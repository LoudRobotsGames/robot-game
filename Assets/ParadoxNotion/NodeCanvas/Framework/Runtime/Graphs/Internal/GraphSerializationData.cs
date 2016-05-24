using System.Collections.Generic;
using UnityEngine;


namespace NodeCanvas.Framework.Internal{

	///The object used to serialize and deserialize graphs. This class serves no other purpose
	[System.Serializable]
	public class GraphSerializationData {

		private readonly float SerializationVersion = 2.3f;

		public float version;
		public System.Type type;
		public string name;
		public string comments;
		public Vector2 translation = new Vector2(-5000, -5000);
		public float zoomFactor = 1f;
		public List<Node> nodes;
		public List<Connection> connections;
		public Node primeNode;
		public List<CanvasGroup> canvasGroups;
		public BlackboardSource localBlackboard;

		//required
		public GraphSerializationData(){}

		//Construct
		public GraphSerializationData(Graph graph){

			this.version         = SerializationVersion;
			this.type            = graph.GetType();
			this.name            = graph.name;
			this.comments        = graph.graphComments;
			this.translation     = graph.translation;
			this.zoomFactor      = graph.zoomFactor;
			this.nodes           = graph.allNodes;
			this.canvasGroups    = graph.canvasGroups;
			this.localBlackboard = graph.localBlackboard;

			var structConnections = new List<Connection>();
			for (var i = 0; i < nodes.Count; i++){
				if (nodes[i] is ISerializationCallbackReceiver){
					(nodes[i] as ISerializationCallbackReceiver).OnBeforeSerialize();
				}

				for (var j = 0; j < nodes[i].outConnections.Count; j++){
					structConnections.Add(nodes[i].outConnections[j]);
				}
			}

			this.connections = structConnections;
			this.primeNode   = graph.primeNode;
		}

		///MUST reconstruct before using the data
		public void Reconstruct(Graph graph){

			//check serialization versions here in the future?

			//re-link node connections
			for (var i = 0; i < this.connections.Count; i++){
				connections[i].sourceNode.outConnections.Add(connections[i]);
				connections[i].targetNode.inConnections.Add(connections[i]);
			}

			//re-set the node's owner and on after deserialize for nodes that need it
			for (var i = 0; i < this.nodes.Count; i++){
				nodes[i].graph = graph;
				if (nodes[i] is ISerializationCallbackReceiver){
					(nodes[i] as ISerializationCallbackReceiver).OnAfterDeserialize();
				}
			}
		}
	}
}