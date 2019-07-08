FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

export const LEGO_BOUNDS = 
{
    (unitless) : [1, 1, 25]
} as IntegerBoundSpec;

/**
 * brickBody - Function that sketches and extrudes brick base, studs on top, dimples within those studs, and text on top
 *  @param context - FeatureScript local context
 *  @param id - FeatureScript local ID
 *  @param {integer} total_width - Total width of brick
 *  @param {integer} total_len - Total length of brick
 *  @param {integer} height_minus_stud - Total height of main brick piece not includeing top studs
 *  @param {integer} t_col - total number of columns
 *  @param {integer} t_row - total number of rows
 *  @param {integer} stud_diameter - Diameter of the studs on top of brick base
 *  @param {integer} inner_dimple_diam - Diameter of the remove exturde on the underside of the top studs
 *  @param {integer} wall_thickness - Depth of the underside dimples
 *  
 */ 
function brickBody(context, id, total_width, total_len, height_minus_stud, t_col, t_row, stud_diameter, inner_dimple_diam, wall_thickness)
{
    //Sketch the main brick body and extrude
    var buildTop = newSketch(context, id + "buildTop", {
            "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
    });
    skRectangle(buildTop, "rectangle1", {
            "firstCorner" : vector(0,0) * millimeter, //vector(-(total_width/2),-(total_len/2)) * millimeter,
            "secondCorner" : vector(total_width, total_len) * millimeter //vector(total_width/2, total_len/2) * millimeter
    });
    skSolve(buildTop);
    extrude(context, id + "bodyExtrude", {
            "entities" : qSketchRegion(id + "buildTop"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.NEW,
            "defaultScope" : false,
            "depth" : height_minus_stud * millimeter
    });
    
    //Sketch Studs on top and extrude
    var studs = newSketch(context, id + "studPlane", {
            "sketchPlane" : qNthElement(qCreatedBy(id + "bodyExtrude", EntityType.FACE),2)
    });
    var text = newSketch(context, id + "text", {
            "sketchPlane" : qNthElement(qCreatedBy(id + "bodyExtrude", EntityType.FACE),2)
    });
    var dimp = newSketch(context, id + "dimpPlane", {
            "sketchPlane" : qNthElement(qCreatedBy(id + "bodyExtrude", EntityType.FACE),2)
    });
    
    var c;
    var r;
    var studCount = 0;
    for (c = 0; c < t_col; c += 1)
    {
        for (r = 0; r < t_row; r += 1)
        {
            var newCalc = vector((-total_width/2)+(4*(1+(2*c))), (-total_len/2)+(4*(1+(2*r))));
            var oldCalc = vector(((4*(c+1))+(c*4)),((4*(r+1))+(r*4)));
            skCircle(studs, "studs" ~ studCount, {
                    "center" : oldCalc * millimeter,
                    "radius" : stud_diameter/2 * millimeter
            });
            skCircle(dimp, "dimple" ~ studCount, {
                    "center" : oldCalc * millimeter,
                    "radius" : inner_dimple_diam/2
            });
            var studDist = 4;
            var studStart = vector(1, 0) * studDist + vector(0, 1) * studDist;
            var legoTopLeft = vector(1, 0) * stud_diameter / (2.25);
            var legoBottomRight = vector(0, 1) * stud_diameter / (8);
            skText(text, "logo_sk" ~ studCount, {
                    "text" : "LEGU",
                    "fontName" : "DroidSansMono-Regular.ttf",
                    "firstCorner" : (studStart + (vector(1, 0) * studDist * 2 * c - legoTopLeft) + (vector(0, 1) * studDist * 2 * r + legoBottomRight)) * millimeter,
                    "secondCorner" : (studStart + (vector(1, 0) * studDist * 2 * c + legoTopLeft) + (vector(0, 1) * studDist * 2 * r - legoBottomRight)) * millimeter
            });
            studCount += 1;
        } 
    }
    skSolve(studs);
    skSolve(dimp);
    skSolve(text);
    extrude(context, id + "studExtrude", {
            "entities" : qSketchRegion(id + "studPlane"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.ADD,
            "defaultScope" : false,
            "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
            "depth" : wall_thickness * millimeter
    });
    extrude(context, id + "innerStud", {
            "entities" : qSketchRegion(id + "dimpPlane"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.REMOVE,
            "oppositeDirection" : true,
            "defaultScope" : false,
            "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
            "depth" : wall_thickness * millimeter
    });
    extrude(context, id + "textEx", {
            "entities" : qSketchRegion(id + "text"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.ADD,
            "oppositeDirection" : false,
            "defaultScope" : false,
            "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
            "depth" : 1.7 * millimeter
    });
}

/**
 * removeUnderside - Function that sketches and remove extrudes (hollow out) inside of brick, reveling dimples underneath studs and making room for a post or cylinder
 *  @param context - FeatureScript local context
 *  @param id - FeatureScript local ID
 *  @param {integer} total_width - Total width of brick
 *  @param {integer} total_len - Total length of brick
 *  @param {integer} wall_thickness - Wall thickness on each side of brickBase
 *  @param {integer} height_minus_stud - Total height of main brick piece not includeing top studs 
 */ 
function removeUnderside(context, id, total_width, total_len, wall_thickness, height_minus_stud)
{
    //Create and remove-extrude inner Base
    var buildBottom = newSketch(context, id + "base", {
            "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
    });
    var newBase = vector((-total_width/2) + wall_thickness, (-total_len/2) + wall_thickness);
    skRectangle(buildBottom, "baseSketch", {
            "firstCorner" : vector(wall_thickness, wall_thickness) * millimeter,
            "secondCorner" : vector(total_width - wall_thickness, total_len - wall_thickness) * millimeter
    });
    skSolve(buildBottom);
    extrude(context, id + "createBase", {
            "entities" : qSketchRegion(id + "base"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.REMOVE,
            "defaultScope" : false,
            "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
            "depth" : (height_minus_stud - wall_thickness) * millimeter
    });
}

/**
 * postCylinder - Function that sketches and extrudes an underside post or cylinder on brick
 *  @param context - FeatureScript local context
 *  @param id - FeatureScript local ID
 *  @param {integer} total_width - Total width of brick
 *  @param {integer} total_len - Total length of brick
 *  @param {integer} t_col - total number of columns
 *  @param {integer} t_row - total number of rows
 *  @param {integer} inner_dimple_diam - Diameter of the remove exturde on the underside of the top studs
 *  @param {integer} height_minus_stud - Total height of main brick piece not includeing top studs 
 *  @param {integer} stud_diameter - Diameter of the studs on top of brick base
 *  @param {integer} cylinder_diam - Diameter of large outer cylinder 
 *  @param {integer} wall_thickness - Depth of the underside dimples 
 */ 
function postCylinder(context, id, total_width, total_len, t_row, t_col, inner_dimple_diam, height_minus_stud, stud_diameter, cylinder_diam, wall_thickness)
{
    var c;
    var r;
    //Create Post or Cylinders and Extrude
    if (t_row > 1 || t_col > 1)
    {
        var i = 0;
        //Create POST for a piece that has side len of 1
        if (t_row == 1 || t_col == 1)
        {
            var posts = newSketch(context, id + "postPlane", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var numOfPosts;
            if (t_row == 1)
                numOfPosts = t_col - 1;
            else
                numOfPosts = t_row - 1;
            while (i < numOfPosts)
            {
                var old = vector(8 + (i * 8), 4);
                var ne = vector((-total_width/2) + (8 * (i + 1)) ,0);
                if (t_row == 1)
                {
                    
                    skCircle(posts, "post" ~ i, {
                            "center" : vector(8 + (i * 8), 4) * millimeter,
                            "radius" : inner_dimple_diam/2
                    });
                }
                else
                {
                    skCircle(posts, "post" ~ i, {
                            "center" : vector(4, 8 + (i * 8)) * millimeter,
                            "radius" : inner_dimple_diam/2
                    });
                }
                i += 1;
            }
            skSolve(posts);
            extrude(context, id + "postExtrude", {
                    "entities" : qSketchRegion(id + "postPlane"),
                    "endBound" : BoundingType.BLIND,
                    "operationType" : NewBodyOperationType.ADD,
                    "defaultScope" : false,
                    "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
                    "depth" : height_minus_stud * millimeter
            });
        }
        
        //Otherwise create cylinders: Extrude Outer cylinder and remove-extrude inner cylinder
        else
        {
            var in_cylinders = newSketch(context, id + "incylinderPlane", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var out_cylinders = newSketch(context, id + "outcylinderPlane", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            var numOfCyl = 0 ;
            if (t_row > t_col)
            {
                for(r = 0; r < t_row - 1 ; r += 1)
                {
                    for (c = 0; c < t_col - 1; c += 1)
                    {
                        var newC = vector((-total_width/2) + ((c + 1) * 8), ((-total_len/2)  + (8 * (r + 1))));
                        var old = vector(8 + (8 * c), 8 + (r * 8));
                        skCircle(in_cylinders, "inner_cyl" ~ numOfCyl, {
                                "center" :  old * millimeter,
                                "radius" : stud_diameter/2 * millimeter
                        });
                        skCircle(out_cylinders, "outer_cyl" ~ numOfCyl, {
                                "center" : old * millimeter,
                                "radius" : cylinder_diam/2
                        });
                        numOfCyl += 1;
                    }
                }
            }
            else
            {
                for(c = 0; c < t_col - 1 ; c += 1)
                {
                    for (r = 0; r < t_row - 1; r += 1)
                    {
                        var test = vector(((-total_width/2)  + (8 * (c + 1))), (-total_len/2) + ((r + 1) * 8));
                        var old = vector(8 + (8 * c), 8 + (r * 8));
                        skCircle(in_cylinders, "inner_cyl" ~ numOfCyl, {
                                "center" : old * millimeter,
                                "radius" : stud_diameter/2 * millimeter
                        });
                        skCircle(out_cylinders, "outer_cyl" ~ numOfCyl, {
                                "center" : old * millimeter,
                                "radius" : cylinder_diam/2
                        });
                        numOfCyl += 1;
                    }
                }
            }
            skSolve(in_cylinders);
            skSolve(out_cylinders);
            extrude(context, id + "extrudeOuter", {
                    "entities" : qSketchRegion(id + "outcylinderPlane"),
                    "endBound" : BoundingType.BLIND,
                    "operationType" : NewBodyOperationType.ADD,
                    "defaultScope" : false,
                    "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
                    "depth" : height_minus_stud * millimeter
            });       
            extrude(context, id + "extrudeInner", {
                    "entities" : qSketchRegion(id + "incylinderPlane"),
                    "endBound" : BoundingType.BLIND,
                    "operationType" : NewBodyOperationType.REMOVE,
                    "defaultScope" : false,
                    "booleanScope" : qUnion([qCreatedBy(id + "bodyExtrude", EntityType.BODY)]),
                    "depth" : (height_minus_stud - wall_thickness) * millimeter
            });
        }
    }
}

/**
 *  CreateLego - Creates LEGO Piece of specified length & width
 *  @author: Shane Poole
 *  @version: 1.0
 *  @param {number} width/columns - One side of lego rectangle
 *  @param {number} length/row - Size of other rectangular lego piece side
 */
annotation { "Feature Type Name" : "CreateLego" }
export const createLego = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Width #:"}
        isInteger(definition.col, LEGO_BOUNDS);
        
        annotation { "Name" : "Len #:"}
        isInteger(definition.row, LEGO_BOUNDS);
    }
    {
        // Default LEGO Measurments
        var side_length = 8;
        var stud_diameter = 4.8;
        var wall_thickness = 1.6;
        var height_minus_stud = 9.6;
        var total_height = 11.2 * millimeter;
        var stud_to_side = 4 * millimeter;
        var inner_dimple_diam = 3.2 * millimeter;
        var cylinder_diam = 6.4 * millimeter;
        var post_diam = 3.2 * millimeter;
        var t_col = definition.col;
        var t_row = definition.row;
        
        var total_width = t_col * side_length;
        var total_len = t_row * side_length;
        var numOfStuds = t_col * t_row;
        
        brickBody(context, id, total_width, total_len, height_minus_stud, t_col, t_row, stud_diameter, inner_dimple_diam, wall_thickness);
        removeUnderside(context, id, total_width, total_len, wall_thickness, height_minus_stud);
        postCylinder(context, id, total_width, total_len, t_row, t_col, inner_dimple_diam, height_minus_stud, stud_diameter, cylinder_diam, wall_thickness);
        
        //Clean Up Sketches
        opDeleteBodies(context, id + "cleanUpSketchs", {
            "entities" : qSketchFilter(qEverything(), SketchObject.YES)
        });
    });

