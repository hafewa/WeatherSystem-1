using UnityEngine;
using System;
using System.Collections;

namespace QuadTreeLib{
	[Serializable]
	public class Item : IHasRect  {

		private MeshRenderer _node;

		public Item(MeshRenderer node){
			_node = node;
		}


		public RectangleF Rectangle {
			get {
				var pos = new Vector2 (_node.bounds.min.x, _node.bounds.min.z);
				var size = new Vector2 (_node.bounds.size.x, _node.bounds.size.z);
				return new RectangleF (pos,size);
			}
		}

		public MeshRenderer value{
			get { 
				return _node;
			}
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != typeof(Item))
				return false;
			Item other = (Item)obj;
			return _node == other._node;
		}

		public override int GetHashCode ()
		{
			return _node.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("{0}=(minX={1},minY={2},maxX={3},maxY={4})", _node.name,_node.bounds.min.x,_node.bounds.min.z,_node.bounds.max.x,_node.bounds.max.z);
		}
		 

	}
}


