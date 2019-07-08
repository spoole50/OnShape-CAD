FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

/* resources
 ** https://web.archive.org/web/20140814000444/http://www.had2know.com/academics/inner-circular-ring-radii-formulas-calculator.html
 ** https://math.stackexchange.com/questions/666491/three-circles-within-a-larger-circle
 ** https://en.wikipedia.org/wiki/Circle_packing_in_a_circle
 */

/**
 * Calcualtes radius of a circle based on a larger enclosing circle radius and how many instances of the smaller cicle would fit tangent
 * @param {number} bigRad -> Radius of the larger circle to filled with smaller circles
 * @param {number} numOfCir -> Number of smaller circles that you want fitted into the larger circle
 */
function calculateRad(bigRad, numOfCir)
{
    var sinPi = sin((PI) / numOfCir * radian);
    var rad = bigRad * sinPi / (sinPi + 1);
    return (rad);
}

/**
 * Rotates current x,y point 120 degress
 *
 * @param {number} curX -> X coordinate to be rotated
 * @param {number} curY -> Y coordinate to be rotated
 * @return {num array} -> num[2] of the new [x,y] coordinates
 */
function rot120(curX, curY)
{
    var s120 = sin(120 * degree);
    var c120 = cos(120 * degree);
    var newX = ((curX * c120) - (curY * s120));
    var newY = ((curY * c120) + (curX * s120));
    return ([newX, newY]);
}

/**
 * Draws 3 circles each rotated 120 degrees from the previous
 *
 * @param {number} sx -> starting x coordinate of circle center
 * @param {number} sy -> starting y coordinate of circle center
 * @param {number} radius -> Radius for all circles to be created
 * @param {var} sketch -> active sketch plane the circle will be attached to
 * @param {string} cirName -> Text of circle name for ID purposes
 */
function makeThreeCircles(sx, sy, radius, sketch, cirName, constToMain)
{
    var i;
    var vec = [sx, sy];
    for (i = 1; i <= 3; i += 1)
    {
        skCircle(sketch, cirName ~ i, {
                    "center" : vector(vec[0], vec[1]) * millimeter,
                    "radius" : radius * millimeter
                });
        if (constToMain == "y")
        {
            skConstraint(sketch, cirName ~ "mainConst" ~ i, {
                        "constraintType" : ConstraintType.TANGENT,
                        "localFirst" : "mainCircle",
                        "localSecond" : cirName ~ i
                    });
        }
        vec = rot120(vec[0], vec[1]);
    }
}

/**
 * circleMaker - Given a circle radius, fill circle with tangent smaller circles
 * @author: Shane Poole
 * @version: 2.0
 * @param {number} rad -> radius of largest outer circle to be filled
 */
annotation { "Feature Type Name" : "circleMaker 2.0" }
export const makeCircles = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "cir_radius" }
        isInteger(definition.rad, POSITIVE_COUNT_BOUNDS);
    }
    {
        //Starting Radius Estimations
        var mainRad = definition.rad;
        var largeThreeRad = calculateRad(mainRad, 3);
        var medEleRad = calculateRad(mainRad, 11);
        var smInnerRad = calculateRad(mainRad, 56);
        var cirXrad = calculateRad(mainRad, 26);
        var cirYrad = calculateRad(mainRad, 47);
        var cirZrad = calculateRad(mainRad, 78);

        //Starting X Y Coordinate Estimations
        var large3x = (116.0254 / 250) * mainRad;
        var large3y = (66.9873 / 250) * mainRad;
        var med3x = 0;
        var med3y = (193.99769 / 250) * mainRad;
        var smInnerX = 0;
        var smInnerY = (124.51612 / 250) * mainRad;

        var cirXx = (82.57271 / 250) * mainRad;
        var cirXy = (206.58464 / 250) * mainRad;
        var cirYx = (124.91329 / 250) * mainRad;
        var cirYy = (198.32648 / 250) * mainRad;
        var cirZx = (148.52243 / 250) * mainRad;
        var cirZy = (188.64883 / 250) * mainRad; 

        //Create Main Sketch and Circle
        var canvas = newSketch(context, id + "mainSketch", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
        skCircle(canvas, "mainCircle", {
                    "center" : vector(0, 0) * millimeter,
                    "radius" : mainRad * millimeter
                });
        skConstraint(canvas, "fixMain", {
                    "constraintType" : ConstraintType.FIX,
                    "localFirst" : "mainCircle"
                });

        //Add all additional circles inside
        makeThreeCircles(large3x, large3y, largeThreeRad, canvas, "largeCir", "y");
        makeThreeCircles(med3x, med3y, medEleRad, canvas, "medCir", "y");
        makeThreeCircles(smInnerX, smInnerY, smInnerRad, canvas, "smInnerCir", "n");
        makeThreeCircles(cirXx, cirXy, cirXrad, canvas, "cirXR", "y");
        makeThreeCircles(-cirXx, cirXy, cirXrad, canvas, "cirXL", "y");
        makeThreeCircles(cirYx, cirYy, cirYrad, canvas, "cirYR", "y");
        makeThreeCircles(-cirYx, cirYy, cirYrad, canvas, "cirYL", "y");
        makeThreeCircles(cirZx, cirZy, cirZrad, canvas, "cirZR", "y");
        makeThreeCircles(-cirZx, cirZy, cirZrad, canvas, "cirZL", "y");
        

        //SET CONSTRAINTS
        //Large Cir contraint to eachother
        skConstraint(canvas, "constraint1", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "largeCir1",
                    "localSecond" : "largeCir" ~ 2,
                });
        skConstraint(canvas, "constraint2", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "largeCir" ~ 1,
                    "localSecond" : "largeCir" ~ 3,
                });
        skConstraint(canvas, "constraint3", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "largeCir" ~ 3,
                    "localSecond" : "largeCir" ~ 2,
                });

        //medCirConstraints
        skConstraint(canvas, "constraint4", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 1,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint5", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 1,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint6", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 2,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint7", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 2,
                    "localSecond" : "largeCir" ~ 3
                });
        skConstraint(canvas, "constraint8", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 3,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint9", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "medCir" ~ 3,
                    "localSecond" : "largeCir" ~ 3
                });

        //smInnerConst
        skConstraint(canvas, "constraint10", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 1,
                    "localSecond" : "medCir" ~ 1
                });
        skConstraint(canvas, "constraint11", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 1,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint12", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 1,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint13", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 2,
                    "localSecond" : "medCir" ~ 2
                });
        skConstraint(canvas, "constraint14", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 2,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint15", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 2,
                    "localSecond" : "largeCir" ~ 3
                });
        skConstraint(canvas, "constraint16", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 3,
                    "localSecond" : "medCir" ~ 3
                });
        skConstraint(canvas, "constraint17", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 3,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint18", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "smInnerCir" ~ 3,
                    "localSecond" : "largeCir" ~ 3
                });

        //cirXR
        skConstraint(canvas, "constraint19", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 1,
                    "localSecond" : "medCir" ~ 1
                });
        skConstraint(canvas, "constraint20", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 1,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint21", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 2,
                    "localSecond" : "medCir" ~ 2
                });
        skConstraint(canvas, "constraint22", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 2,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint23", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 3,
                    "localSecond" : "medCir" ~ 3
                });
        skConstraint(canvas, "constraint24", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXR" ~ 3,
                    "localSecond" : "largeCir" ~ 3
                });

        //cirXL
        skConstraint(canvas, "constraint25", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 1,
                    "localSecond" : "medCir" ~ 1
                });
        skConstraint(canvas, "constraint26", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 1,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint27", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 2,
                    "localSecond" : "medCir" ~ 2
                });
        skConstraint(canvas, "constraint28", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 2,
                    "localSecond" : "largeCir" ~ 3
                });
        skConstraint(canvas, "constraint29", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 3,
                    "localSecond" : "medCir" ~ 3
                });
        skConstraint(canvas, "constraint30", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirXL" ~ 3,
                    "localSecond" : "largeCir" ~ 1
                });

        //cirYR
        skConstraint(canvas, "constraint31", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 1,
                    "localSecond" : "cirXR" ~ 1
                });
        skConstraint(canvas, "constraint32", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 1,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint33", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 2,
                    "localSecond" : "cirXR" ~ 2
                });
        skConstraint(canvas, "constraint34", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 2,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint35", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 3,
                    "localSecond" : "cirXR" ~ 3
                });
        skConstraint(canvas, "constraint36", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYR" ~ 3,
                    "localSecond" : "largeCir" ~ 3
                });

        //cirYL
        skConstraint(canvas, "constraint37", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 1,
                    "localSecond" : "cirXL" ~ 1
                });
        skConstraint(canvas, "constraint38", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 1,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint39", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 2,
                    "localSecond" : "cirXL" ~ 2
                });
        skConstraint(canvas, "constraint40", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 2,
                    "localSecond" : "largeCir" ~ 3
                });
        skConstraint(canvas, "constraint41", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 3,
                    "localSecond" : "cirXL" ~ 3
                });
        skConstraint(canvas, "constraint42", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirYL" ~ 3,
                    "localSecond" : "largeCir" ~ 1
                });

        //cirZR
        skConstraint(canvas, "constraint43", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 1,
                    "localSecond" : "cirYR" ~ 1
                });
        skConstraint(canvas, "constraint44", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 1,
                    "localSecond" : "largeCir" ~ 1
                });
        skConstraint(canvas, "constraint45", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 2,
                    "localSecond" : "cirYR" ~ 2
                });
        skConstraint(canvas, "constraint46", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 2,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint47", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 3,
                    "localSecond" : "cirYR" ~ 3
                });
        skConstraint(canvas, "constraint48", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZR" ~ 3,
                    "localSecond" : "largeCir" ~ 3
                });

        //cirZL
        skConstraint(canvas, "constraint49", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 1,
                    "localSecond" : "cirYL" ~ 1
                });
        skConstraint(canvas, "constraint50", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 1,
                    "localSecond" : "largeCir" ~ 2
                });
        skConstraint(canvas, "constraint51", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 2,
                    "localSecond" : "cirYL" ~ 2
                });
        skConstraint(canvas, "constraint52", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 2,
                    "localSecond" : "largeCir" ~ 3
                });
        skConstraint(canvas, "constraint53", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 3,
                    "localSecond" : "cirYL" ~ 3
                });
        skConstraint(canvas, "constraint54", {
                    "constraintType" : ConstraintType.TANGENT,
                    "localFirst" : "cirZL" ~ 3,
                    "localSecond" : "largeCir" ~ 1
                });
        skSolve(canvas);
    });

