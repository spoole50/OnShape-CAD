FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "Draw2.8" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
         annotation { "Name" : "base_len" }
            isInteger(definition.baselen, {(unitless) : [75, 88, 95]} as IntegerBoundSpec);
         annotation { "Name" : "base_wid" }
            isInteger(definition.basewid, {(unitless) : [45, 50, 60]} as IntegerBoundSpec);
        annotation { "Name" : "bridge_height" }
            isInteger(definition.bheight, {(unitless) : [45, 56, 65]} as IntegerBoundSpec);
    }
    {
        var len = definition.baselen;
        var width = definition.basewid;
        var q4 = 28/88*len;
        var q2 = 38/50*width;
        var bridge = 12;
        var bridge_height = definition.bheight;
        var base_height = 18/56*bridge_height;
        
        //Create Base Sketch
        var base = newSketch(context, id + "base", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        //Create Bridge Sketch
        var bridgeEx = newSketch(context, id + "bridge", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        //Sketch Base Rectangle
        skRectangle(base, "baseRec", {
                "firstCorner" : vector(0, 0) * millimeter,
                "secondCorner" : vector(len, width) * millimeter
        });
        //Sketch Horizontal Bridge Rectangle
        skRectangle(bridgeEx, "bridgeHoriz", {
                "firstCorner" : vector(0, q4) * millimeter,
                "secondCorner" : vector(len, q4 + bridge) * millimeter
        });
        //Sketch Verticle Bridge Rectangle
        skRectangle(bridgeEx, "bridgeVert", {
                "firstCorner" : vector(q2, 0) * millimeter,
                "secondCorner" : vector(q2 + bridge, width) * millimeter
        });
        //Solve Each Sketch
        skSolve(base);
        skSolve(bridgeEx);
        //Extrude Base to base_height
        extrude(context, id + "extrudeBase", {
                "entities" : qSketchRegion(id + "base"),
                "endBound" : BoundingType.BLIND,
                "depth" : base_height * millimeter
        });
        //Extrude Bridge to bridge_height
        extrude(context, id + "extrudeBridge", {
                "entities" : qSketchRegion(id + "bridge"),
                "endBound" : BoundingType.BLIND,
                "operationType" : NewBodyOperationType.ADD,
                "depth" : bridge_height * millimeter
        });
        //Calcualte Plane through 3 points to interect
        var a = vector(32, 0, bridge_height) * millimeter;
        var b = vector(len, width, bridge_height) * millimeter;
        var c = vector(len,0,0) * millimeter;
        var _origin = (a+b+c)/3;
        var _normal = cross((c-a), (b-a));
        opPlane(context, id + "cutOutPlane", {
                "plane" : plane(_origin, _normal),
                "width" : 100 * millimeter,
                "height" : 100 * millimeter
        });
        //Build sketch on created plane 
        var cutOut = newSketch(context, id + "cutOut", {
                "sketchPlane" : qCreatedBy(id + "cutOutPlane", EntityType.FACE)
        });
        //Sketch large rectangle for extrude
        skRectangle(cutOut, "recEx", {
                "firstCorner" : vector(-100, -100) * millimeter,
                "secondCorner" : vector(100, 100) * millimeter
        });
        skSolve(cutOut);
        //Extrude sketch by removal cutting original piece
        extrude(context, id + "extrudeCut", {
                "entities" : qSketchRegion(id + "cutOut"),
                "endBound" : BoundingType.BLIND,
                "operationType" : NewBodyOperationType.REMOVE,
                "depth" : 50 * millimeter
        });
        //Clean Up Sketches and interesecting plane
        opDeleteBodies(context, id + "cleanUpSketchs", {
                "entities" : qSketchFilter(qEverything(), SketchObject.YES)
        });
        opDeleteBodies(context, id + "cleanPlane", {
                "entities" : qCreatedBy(id + "cutOutPlane", EntityType.FACE)
        });
    });

