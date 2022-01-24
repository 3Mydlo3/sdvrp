import { OnInit, ChangeDetectorRef } from '@angular/core';
import {Component, Inject} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {style} from "@angular/animations";

@Component({
  selector: 'app-sd-vrp',
  templateUrl: './sd-vrp.component.html',
  styleUrls: ['./sd-vrp.component.css']
})

export class SdvrpComponent {
  private static pointsData: MapPoint[] = [
    { demand: 0, position: { x: 250, y: 140}}
  ];
  public get points() { return SdvrpComponent.pointsData; }

  private baseUrl : string;
  private httpClient : HttpClient;

  private static activeItem : HTMLElement = null!;
  private static active : boolean = false;
  private static currentX : number = 0;
  private static currentY : number = 0;
  private static initialPointX : number = 0;
  private static initialPointY : number = 0;
  private static initialX : number = 0;
  private static initialY : number = 0;
  private static xOffset : number = 0;
  private static yOffset : number = 0;

  constructor(
    httpClient: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private changeDetectorRef: ChangeDetectorRef) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;

    document?.addEventListener("DOMContentLoaded", _ => {
      let submitButton = document.getElementById("SubmitButton");
      submitButton?.addEventListener("click", this.submit);

      let container = document.getElementById("MapContainer");
      container!.addEventListener("mousedown", this.dragStart, false);
      container!.addEventListener("mouseup", this.dragEnd, false);
      container!.addEventListener("mousemove", this.drag, false);
    })
  }

  importProblem(event: Event) {
    const element = event?.target as HTMLInputElement;
    let fileList: FileList | null = element.files;
    if (fileList !=  null && fileList.length > 0) {
      let file = fileList.item(0);
      let problemPromise = file!.text()
        .then(text => JSON.parse(text) as Problem)
        .then(problem => {
            problem.nodes = problem.nodes.map<ProblemPoint>(problemPoint => {
              return {
                x: problemPoint.x,
                y: problemPoint.y,
                demand: problemPoint.demand,
                initialDemand: problemPoint.demand,
                id: problemPoint.id
              }
          })
            .sort(x => x.id);

          let missingNodeIds: number[] = [];
          for (let index = 0; index < problem.nodes.length; index++) {
            if (!problem.nodes.some(x => x.id == index)) {
              missingNodeIds.push(index);
            }
          }

          if (missingNodeIds.length > 0) {
            let tooHightNodeIds: number[] = problem.nodes
              .filter(x => x.id >= problem.nodes.length)
              .map(x => x.id);

            alert(`The import file has ${problem.nodes.length} nodes but misses nodes with id: ${missingNodeIds.toString()}, and has nodes with too high id: ${tooHightNodeIds.toString()}`)
          }

          if (problem.nodes.some(x => problem.nodes.filter(node => node.id == x.id).length > 1)) {
            alert("There are node id duplicates in the import file, indexes might be wrong.")
          }
          return problem;
        })
        .then(problem => {
          console.log("Uploaded problem:", JSON.stringify(problem));
          return problem;
        })
        .then(problem => this.setVehicleCapacity(problem))
        .then(problem => this.setCostFunctionType(problem))
        .then(problem => this.setMaxIterations(problem))
        .then(problem => this.setPoints(problem))
        .catch(error => {
          const message = "Error loading problem from file" + error.toString();
          console.error(message);
          alert(message);
        });
    }
  }

  exportProblem(maybeProblem: Problem | null) {
    let problem: Problem = maybeProblem ?? this.getProblem();
    this.handleFileOutput(problem, "export");
  }

  handleFileOutput(object: unknown, fileName: string) {
    let element = document.createElement('a');
    element.setAttribute('href', 'data:application/json;charset=utf-8,' + encodeURIComponent(JSON.stringify(object)));
    element.setAttribute('download', fileName + ".json");

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
  }

  showSolution(object: unknown) {
    let solution = JSON.parse(JSON.stringify(object)) as Solution;
    this.handleFileOutput(solution, "solution");
  }

  getVehicleCapacity(): number {
    return parseInt((document.getElementById("VehicleCapacityInput") as HTMLInputElement).value);
  };

  setVehicleCapacity(problem: Problem): Problem {
    (document.getElementById("VehicleCapacityInput") as HTMLInputElement).value = problem.vehicleCapacity.toString();
    return problem;
  }

  getMaxIterations(): number {
    return parseInt((document.getElementById("MaxIterationsInput") as HTMLInputElement).value);
  };

  setMaxIterations(problem: Problem): Problem {
    (document.getElementById("MaxIterationsInput") as HTMLInputElement).value = problem.maxIterations.toString();
    return problem;
  };

  getCostFunctionType(): number {
    return parseInt((document.getElementById("CostFunctionTypeInput") as HTMLInputElement).value);
  };

  setCostFunctionType(problem: Problem): Problem {
    (document.getElementById("CostFunctionTypeInput") as HTMLInputElement).value = problem.costFunctionType.toString();
    return problem;
  };

  setPoints(problem: Problem): Problem {
    SdvrpComponent.pointsData = problem.nodes
      .sort(x => x.id)
      .map(x => this.convertProblemPointToMap(x));
    return problem;
  }

  convertProblemPointToMap(problemPoint: ProblemPoint): MapPoint {
    return {
      position: {
        x: problemPoint.x,
        y: problemPoint.y
      },
      demand: problemPoint.demand
    }
  }

  getProblemPoints(): ProblemPoint[] {
    let problemPoints: ProblemPoint[] = [];
    for (let i = 0; i < this.points.length; i++) {
      let currentPoint = this.points[i];
      problemPoints.push({
        x: currentPoint.position.x,
        y: currentPoint.position.y,
        demand: currentPoint.demand,
        initialDemand: currentPoint.demand,
        id: i
      })
    }
    return problemPoints;
  }

  getProblem(): Problem {
    return {
      nodes: this.getProblemPoints(),
      vehicleCapacity: this.getVehicleCapacity(),
      maxIterations: this.getMaxIterations(),
      costFunctionType: this.getCostFunctionType()
    }
  }

  submit() {
    let problem: Problem = this.getProblem();

    console.debug(`Submitting problem: ${JSON.stringify(problem)}`)

    this.httpClient.post(this.baseUrl + "problem/solve", JSON.stringify(problem), {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }})
      .subscribe(result => {
        console.log(JSON.stringify(result));
        this.showSolution(result);
      }, error => {

        alert(`${error.status} ${error.statusText}: ${JSON.stringify(error.error.message)}`);
        console.error(`${error.toString()} | Request body ${JSON.stringify(problem)}`);
      });
  }

  updateDemand(event: Event, index: number) {
    let input = event.target as HTMLInputElement;
    let value = parseInt(input.value);
    // let mapPoint = document.getElementById("POINT_" + index) as HTMLSpanElement;
    let pointData = SdvrpComponent.pointsData[index];
    SdvrpComponent.pointsData[index] = {
      demand: value,
      position: {
        x: pointData.position.x,
        y: pointData.position.y
      }
    };
    this.changeDetectorRef.detectChanges();
  }

  positionXUpdate(event: Event, index: number) {
    let input = event.target as HTMLInputElement;
    let value = parseInt(input.value);
    // let mapPoint = document.getElementById("POINT_" + index) as HTMLSpanElement;
    let pointData = SdvrpComponent.pointsData[index];
    SdvrpComponent.pointsData[index] = {
      demand: pointData.demand,
      position: {
        x: value,
        y: pointData.position.y
      }
    };
    this.changeDetectorRef.detectChanges();
    // SdvrpComponent.setTranslate(pointData.position.x, pointData.position.y, mapPoint);
    // mapPoint.style = "left: " + pointData.position.x + "px; top: " + pointData.position.y + "px;"
  }

  positionYUpdate(event: Event, index: number) {
    let input = event.target as HTMLInputElement;
    let value = parseInt(input.value);
    let pointData = SdvrpComponent.pointsData[index];
    SdvrpComponent.pointsData[index] = {
      demand: pointData.demand,
      position: {
        x: pointData.position.x,
        y: value
      }
    };
    this.changeDetectorRef.detectChanges();
  }

  addPoint() {
    SdvrpComponent.pointsData.push({
      position: {
        x: 150,
        y: 150
      },
      demand: 0
    })
  }

  delete(index: number) {
    if (index != 0) {
      SdvrpComponent.RemoveElementFromObjectArray(index, SdvrpComponent.pointsData);
    }
  }

  dragStart(mouseEvent : MouseEvent) {
    if (mouseEvent.target !== mouseEvent.currentTarget && SdvrpComponent.activeItem == null) {

      if (mouseEvent.target instanceof Element) {
        let element = mouseEvent.target as Element;
        SdvrpComponent.activeItem = element as HTMLElement;
        if (element.classList.contains("map-point")) {
          SdvrpComponent.active = true;

          let pointId = SdvrpComponent.getPointIdFromHtmlElement(SdvrpComponent.activeItem);
          SdvrpComponent.initialPointX = SdvrpComponent.pointsData[pointId].position.x;
          SdvrpComponent.initialPointY = SdvrpComponent.pointsData[pointId].position.y;

          SdvrpComponent.initialX = mouseEvent.clientX - SdvrpComponent.xOffset;
          SdvrpComponent.initialY = mouseEvent.clientY - SdvrpComponent.yOffset;
        }
      }

    }
  }

  dragEnd(mouseEvent : MouseEvent) {
    SdvrpComponent.active = false;
    SdvrpComponent.activeItem = null!;
    SdvrpComponent.currentX = 0;
    SdvrpComponent.currentY = 0;
    SdvrpComponent.initialX = 0;
    SdvrpComponent.initialY = 0;
    SdvrpComponent.xOffset = 0;
    SdvrpComponent.yOffset = 0;
  }

  drag(mouseEvent : MouseEvent) {
    if (SdvrpComponent.active) {

      mouseEvent.preventDefault();

      // TODO: Disallow moving point out of the map
      SdvrpComponent.currentX = mouseEvent.clientX - SdvrpComponent.initialX;
      SdvrpComponent.currentY = mouseEvent.clientY - SdvrpComponent.initialY;

      SdvrpComponent.xOffset = SdvrpComponent.currentX;
      SdvrpComponent.yOffset = SdvrpComponent.currentY;

      let htmlElement = SdvrpComponent.activeItem;
      if (!htmlElement.classList.contains("map-point")) return;

      let pointId = SdvrpComponent.getPointIdFromHtmlElement(htmlElement);
      SdvrpComponent.pointsData[pointId] = {
        demand: SdvrpComponent.pointsData[pointId].demand,
        position: {
          x: SdvrpComponent.initialPointX + SdvrpComponent.currentX,
          y: SdvrpComponent.initialPointY + SdvrpComponent.currentY
        }
      }

      SdvrpComponent.setTranslate(SdvrpComponent.currentX, SdvrpComponent.currentY, htmlElement);
    }
  }

  static getPointIdFromHtmlElement(htmlElement: HTMLElement) {
    return parseInt(htmlElement.id.slice("POINT_".length));
  }

  static setTranslate(xPos : number, yPos : number, el : HTMLElement) {
    el.style.transform = "translate3d(" + xPos + "px, " + yPos + "px, 0)";
  }

  static RemoveElementFromObjectArray(key: number, objectArray: MapPoint[]) {
    objectArray.forEach((value, index) => {
      if (index == key) objectArray.splice(index,1);
    });
  }
}

interface Solution {
  cost: number,
  vehicles: Vehicle[]
}

interface Vehicle {
  id: number,
  capacity: number,
  freeCapacity: number,
  nodes: VisitedPoint[]
}

interface VisitedPoint {
  load: number,
  node: ProblemPoint
}

interface Problem {
  nodes: ProblemPoint[],
  vehicleCapacity: number,
  maxIterations: number,
  costFunctionType: number
}

interface MapPoint {
  demand: number;
  position: Position
}

interface ProblemPoint {
  demand: number,
  initialDemand: number,
  x: number,
  y: number,
  id: number
}

interface Position {
  x: number;
  y: number;
}
