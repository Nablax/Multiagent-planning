using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils{
    public class BasicShape{
        public Vector3 origin;
    }
    public class Circle: BasicShape{
        public float radius;
        public Circle(){
            origin = new Vector3(0, 0, 0);
            radius = 0;
        }
        public Circle(Vector3 inCenter, float inRadius){
            origin = inCenter;
            radius = inRadius;
        }
    }

    public class Rectangle: BasicShape{
        public Vector2 sideLength;
        public Rectangle(){
            origin = new Vector3(0, 0, 0);
            sideLength = new Vector2(0, 0);
        }
    }

    public class Ray2D: BasicShape{
        public Vector3 end, dir;
        public float maxT;
        public Ray2D(){
            origin = new Vector3(0, 0, 0);
            end = new Vector3(0, 0, 0);
            dir = new Vector3(0, 0, 0);
            maxT = 0;
        }
        public void updateRay(Vector3 start, Vector3 end){
            this.origin = start;
            this.end = end;
            this.maxT = 0;
            if(start != end){
                this.dir = (end - start).normalized;
                this.maxT = (end - start).magnitude;
            }
        }
    }
    class RenderUtils{
        public static void renderLines(Vector3 start, Vector3 end, Color lineColor){
            GL.Begin(GL.LINE_STRIP);
            GL.Color(lineColor);
            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x, end.y, end.z);
            GL.End();
        }

        public static Color IdxMapColor(float idx, float totalIdx){
            Color retColor = new Color();
            float prop = 3.0f * idx / totalIdx;
            retColor.r = Mathf.Min(prop, 1);
            prop -= 1; prop = Mathf.Max(0, prop);
            retColor.g = Mathf.Min(prop, 1);
            prop -= 1; prop = Mathf.Max(0, prop);
            retColor.b = Mathf.Min(prop, 1);
            retColor.a = 1;
            return retColor;
        }
    }

    class CollisionCheck{
        public static float clamp(float value, float lowerBound, float upperBound){
            return Mathf.Max(lowerBound, Mathf.Min(upperBound, value));
        }
        public static bool circleAABB2D(Rectangle obstacle, Circle agent){
            float y = agent.origin.y;
            var obstacleToAgent = agent.origin - obstacle.origin;
            obstacleToAgent.y = y;
            var closest = obstacleToAgent - new Vector3(clamp(obstacleToAgent.x, -obstacle.sideLength.x / 2, obstacle.sideLength.x / 2), y, clamp(obstacleToAgent.z, -obstacle.sideLength.y / 2, obstacle.sideLength.y / 2));
            return closest.magnitude < agent.radius;
        }
        public static bool rayAABB2D(Rectangle obstacle, Ray2D ray, float radius){
            if(ray.maxT == 0 ){
                return false;
            }
            float leftSideT, rightSideT, topSideT, botSideT;
            leftSideT = (obstacle.origin.x - 0.5f * obstacle.sideLength.x - radius - ray.origin.x) / ray.dir.x;
            rightSideT = (obstacle.origin.x + 0.5f * obstacle.sideLength.x + radius - ray.origin.x) / ray.dir.x;
            topSideT = (obstacle.origin.z - 0.5f * obstacle.sideLength.y - radius - ray.origin.z) / ray.dir.z;
            botSideT = (obstacle.origin.z + 0.5f * obstacle.sideLength.y + radius - ray.origin.z) / ray.dir.z;
            float maxT = Mathf.Min(Mathf.Max(leftSideT, rightSideT), Mathf.Max(topSideT, botSideT));
            float minT = Mathf.Max(Mathf.Min(leftSideT, rightSideT), Mathf.Min(topSideT, botSideT));

            if(maxT < 0 || minT > maxT || minT > ray.maxT){
                return false;
            }
            return true;
        }
    }

}
