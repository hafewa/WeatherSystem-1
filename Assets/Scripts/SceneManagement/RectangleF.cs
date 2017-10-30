using UnityEngine;
using System.Collections;

public class RectangleF  { 

	private Rect _rect;

	public RectangleF(Rect rect){
		this._rect = rect; 
	}

	public RectangleF(Vector2 position,Vector2 size){
		_rect.position = position;
		_rect.size = size;
	}

	public bool IsEmpty{
		get { 
			return Mathf.Approximately(Width,0) || Mathf.Approximately(Height,0);
		}
	}

	public bool IntersectsWith(RectangleF other){
		return _rect.Overlaps (other._rect);
	}

	public bool Contains(RectangleF other){
		return _rect.Contains (other._rect.min) && _rect.Contains (other._rect.max);
	}

	public Vector2 Center{
		get { 
			return _rect.center;
		}
	}

	public Vector2 Location{
		get { 
			return _rect.min;
		}
	}

	public float Left{
		get { 
			return _rect.xMin;
		}
	}

	public float Top{
		get { 
			return _rect.yMin;
		}
	}

	public float Width{
		get { 
			return _rect.width;
		}
	}

	public float Height{
		get { 
			return _rect.height;
		}
	}

	public override string ToString ()
	{
		return string.Format ("(minX={0},minY={1},maxX={2},maxY={3})", _rect.min.x,_rect.min.y,_rect.max.x,_rect.max.y);
	}
	
} 