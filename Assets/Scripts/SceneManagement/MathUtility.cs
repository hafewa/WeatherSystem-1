using UnityEngine;
using System.Collections;

public enum PointLocation{
	IN_RANGE,
	OUT_RANGE,
	IN_EDGE,
}

public abstract class MathUtility {

	public static float GetDistance(Rect box,Vector3 pos){
		float distance = 0;
		if (pos.x <= box.xMin) {
			if (pos.z < box.yMin) {
				distance = Mathf.Sqrt ((box.xMin - pos.x)*(box.xMin - pos.x)+(box.yMin - pos.z)*(box.yMin - pos.z));
			} else if (pos.z > box.yMax) {
				distance = Mathf.Sqrt ((box.xMin - pos.x)*(box.xMin - pos.x)+(box.yMax - pos.z)*(box.yMax - pos.z));
			} else {
				distance = box.xMin - pos.x;
			}
		} else if (pos.x >= box.xMax) {
			if (pos.z < box.yMin) {
				distance = Mathf.Sqrt ((box.xMax - pos.x)*(box.xMin - pos.x)+(box.xMax - pos.z)*(box.yMin - pos.z));
			} else if (pos.z > box.yMax) {
				distance = Mathf.Sqrt ((box.xMax - pos.x)*(box.xMin - pos.x)+(box.xMax - pos.z)*(box.yMax - pos.z));
			} else {
				distance = pos.x - box.xMax;
			}
		} else if (pos.z <= box.yMin) {
			distance = box.yMin - pos.z;
		} else if (pos.z >= box.yMax) {
			distance = pos.z - box.yMax;
		}

		return distance;
	}	 

	public static PointLocation GetLocation(Rect box,Vector3 pos,Vector3 dir,float edgeSize,out float factor){ 
		if (box.Contains (new Vector2 (pos.x,pos.z))) {
			float nearestX = Mathf.Min (pos.x - box.xMin,box.xMax-pos.x);
			float nearestY = Mathf.Min (pos.y - box.yMin,box.yMax-pos.y);
			float nearest = Mathf.Min (nearestX, nearestY);
			if (nearest < edgeSize) {
				Vector3 interectPoint;
				if (GetInterectPoint (box, pos, dir * 1000, out interectPoint)) {
					var segment = (pos - interectPoint);
					segment.y = 0;
					if (segment.magnitude < edgeSize) {
						factor = 1 + segment.magnitude / (edgeSize); 
						return PointLocation.IN_EDGE;
					}
				}
			}
			factor = 2;
			return PointLocation.IN_RANGE;
		}
		factor = -1;
		return PointLocation.OUT_RANGE;
	}

	static void CheckNearestPoint(ref Vector3 pos,bool interect1,ref Vector3 result1,ref bool interect,ref float minDistance,ref Vector3 result){
		if (interect1) {
			float distance = (result1 - pos).sqrMagnitude;
			if (!interect || distance < minDistance) { 
				minDistance = distance;
				result = result1;
				interect = true;
			}
		}
	}

	public static bool GetSimpleNearestInterectPoint(Rect box,Vector3 pos,Vector3 dir,out Vector3 result){ 		 
		result = Vector3.zero;
		if (!WillInterect (box,pos,dir))
			return false;

		bool interect = false;
	
		if (pos.x < box.xMin && dir.x > 0) {
			interect = true;
			result = new Vector3 (box.xMin,pos.y,pos.z);
		}else if (pos.x > box.xMax && dir.x < 0) {
			interect = true;
			result = new Vector3 (box.xMax,pos.y,pos.z);
		}else if (pos.z < box.yMin && dir.z > 0) {
			interect = true;
			result = new Vector3 (pos.x,pos.y,box.yMin);
		}else if (pos.z > box.yMax && dir.z < 0) {
			interect = true;
			result = new Vector3 (pos.x,pos.y,box.yMax);
		}

		return interect;
	}

	public static bool GetNearestInterectPoint(Rect box,Vector3 pos,Vector3 dir,out Vector3 result){ 		 
		result = Vector3.zero;
		if (!WillInterect (box,pos,dir))
			return false;

		bool interect = false;
		float minDistance = float.MaxValue;		

		bool interect1, interect2, interect3, interect4;
		Vector3 result1,result2,result3,result4;  
		 
		interect1 = GetInterectPoint (box.min, new Vector2 (box.xMin, box.yMax), pos, dir, out result1);
		interect2 = GetInterectPoint (box.min, new Vector2 (box.xMax, box.yMin), pos, dir, out result2);
		interect3 = GetInterectPoint (new Vector2 (box.xMin, box.yMax),box.max, pos, dir, out result3);
		interect4 = GetInterectPoint (new Vector2 (box.xMax, box.yMin),box.max, pos, dir, out result4);


		CheckNearestPoint (ref pos,interect1,ref result1,ref interect,ref minDistance,ref result);
		CheckNearestPoint (ref pos,interect2,ref result2,ref interect,ref minDistance,ref result);
		CheckNearestPoint (ref pos,interect3,ref result3,ref interect,ref minDistance,ref result);
		CheckNearestPoint (ref pos,interect4,ref result4,ref interect,ref minDistance,ref result); 

		return interect;
	}

	public static bool GetInterectPoint(Rect box,Vector3 pos,Vector3 dir,out Vector3 result){ 
		result = Vector3.zero;
		if (!WillInterect (box,pos,dir))
			return false;
		 
		if (GetInterectPoint (box.min, new Vector2 (box.xMin, box.yMax), pos, dir, out result)) {
			return true;
		}

		if (GetInterectPoint (box.min, new Vector2 (box.xMax, box.yMin), pos, dir, out result)) {
			return true;
		} 

		if (GetInterectPoint (new Vector2 (box.xMin, box.yMax),box.max, pos, dir, out result)) {
			return true;
		} 

		if (GetInterectPoint (new Vector2 (box.xMax, box.yMin),box.max, pos, dir, out result)) {
			return true;
		} 

		return false;
	}

	private static bool WillInterect(Rect box,Vector3 pos,Vector3 dir){
		if (pos.x < box.xMin && dir.x <= 0) {
			return false;
		}

		if (pos.x > box.xMax && dir.x >= 0) {
			return false;
		}
		 
		if (pos.z < box.yMin && dir.z <= 0) {
			return false;
		}

		if (pos.z > box.yMax && dir.z >= 0) {
			return false;
		}

		return true;
	}

	const float FLOAT_ZERO  = 0.000001f;

	public static bool  GetInterectPoint(Vector2 start,Vector2 end,Vector3 pos,Vector3 dir,out Vector3 result){
		bool hasInterect = false;
		result = Vector2.zero;

		var a = start;
		var b = end;
		var c = new Vector2 (pos.x, pos.z);
		var d = new Vector2 (pos.x + dir.x , pos.z+dir.z);

		var denominator = (b.y - a.y) * (d.x - c.x) - (a.x - b.x) * (c.y - d.y);

		if (!Mathf.Approximately(denominator,0)) {
			var x = ((b.x - a.x) * (d.x - c.x) * (c.y - a.y)
			        + (b.y - a.y) * (d.x - c.x) * a.x
			        - (d.y - c.y) * (b.x - a.x) * c.x) / denominator;  
			var y = -((b.y - a.y) * (d.y - c.y) * (c.x - a.x)
			        + (b.x - a.x) * (d.y - c.y) * a.y
			        - (d.x - c.x) * (b.y - a.y) * c.y) / denominator; 

			if ((x - a.x) * (x - b.x) <= FLOAT_ZERO && (y - a.y) * (y - b.y) <= FLOAT_ZERO && (x - c.x) * (x - d.x) <= FLOAT_ZERO && (y - c.y) * (y - d.y) <= FLOAT_ZERO)  {
				hasInterect = true;
				result = new Vector3 (x, pos.y, y);
			}
		}

		return hasInterect;

	}

}
