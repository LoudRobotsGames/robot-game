using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;


namespace NodeCanvas.BehaviourTrees{

	/// <summary>
	/// Super Base class for BehaviourTree nodes that can live within a BehaviourTree Graph.
	/// </summary>
	abstract public class BTNode : Node {

		sealed public override System.Type outConnectionType{ get{return typeof(BTConnection);} }
		sealed public override int maxInConnections{ get{return 1;} }
		public override int maxOutConnections{ get{return 0;} }
		sealed public override bool allowAsPrime {get{return true;}}
		public override bool showCommentsBottom{ get{return true;}}

		///Add a child node to this node connected to the specified child index
		public T AddChild<T>(int childIndex) where T:BTNode{
			if (outConnections.Count >= maxOutConnections && maxOutConnections != -1)
				return null;
			var child = graph.AddNode<T>();
			graph.ConnectNodes(this, child, childIndex);
			return child;
		}

		///Add a child node to this node connected last
		public T AddChild<T>() where T:BTNode{
			if (outConnections.Count >= maxOutConnections && maxOutConnections != -1)
				return null;
			var child = graph.AddNode<T>();
			graph.ConnectNodes(this, child);
			return child;
		}

		///Fetch all child nodes of the node recursively, optionaly including this.
		///In other words, this fetches the whole branch.
		public List<BTNode> GetAllChildNodesRecursively(bool includeThis){

			var childList = new List<BTNode>();
			if (includeThis)
				childList.Add(this);

			foreach (BTNode child in outConnections.Select(c => c.targetNode))
				childList.AddRange(child.GetAllChildNodesRecursively(true));

			return childList;
		}

		///Fetch all child nodes of this node with their depth in regards to this node.
		///So, first level children will have a depth of 1 while second level a depth of 2
		public Dictionary<BTNode, int> GetAllChildNodesWithDepthRecursively(bool includeThis, int startIndex){

			var childList = new Dictionary<BTNode, int>();
			if (includeThis)
				childList[this] = startIndex;

			foreach (BTNode child in outConnections.Select(c => c.targetNode)){
				foreach (var pair in child.GetAllChildNodesWithDepthRecursively(true, startIndex + 1))
					childList[pair.Key] = pair.Value;
			}

			return childList;
		}
	}
}