#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ParadoxNotion.Design{

	///Proviving a GenericMenu, shows a popup window like the one of Unity's Component
	public class CompleteContextMenu : PopupWindowContent {

		class Node{
			public EditorUtils.MenuItemInfo item = null;
			public Node parent;
			public Dictionary<string, Node> children = new Dictionary<string, Node>();
			public string path {get;set;}
		}

		private string title;
		private EditorUtils.MenuItemInfo[] items;
		private Vector2 scrollPos;
		private string lastSearch;
		private string search;
		readonly private Node rootNode = new Node();
		private Node currentNode = null;
		readonly private Color hoverColor = new Color(0.5f,0.5f,1, 0.3f);
		private GUIStyle headerStyle;
		private float helpRectHeight = 64;
		private int hoveringIndex;
		private Rect mainRect;
		private bool init;
		
		///Shows the popup menu at position and with title
		public static void Show(GenericMenu menu, Vector2 pos, string title){
			PopupWindow.Show( new Rect(pos.x, pos.y, 0, 0), new CompleteContextMenu(menu, title) );
		}

		//init
		public CompleteContextMenu(GenericMenu menu, string title){
			this.items = EditorUtils.GetMenuItems(menu);
			rootNode = new Node();
			currentNode = rootNode;
			this.title = title;
			headerStyle = new GUIStyle("label");
			headerStyle.alignment = TextAnchor.UpperCenter;
			GenerateTree();
		}

		//Generate the tree node structure out of the items
		void GenerateTree(){
			foreach (var item in items){
				var path = item.content.text;
				var parts = path.Split( new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);
				Node current = rootNode;
				foreach (var part in parts){
					Node child = null;
					if (!current.children.TryGetValue(part, out child)){
						child = new Node{path = part, parent = current};
						current.children[part] = child;
						if (part == parts.Last()){
							child.item = item;
						}
					}
					current = child;
				}
			}		
		}

		//...
		public override Vector2 GetWindowSize(){ return new Vector2(600, 600); }


		//Show stuff
		public override void OnGUI(Rect rect){

			var e = Event.current;
			EditorGUIUtility.SetIconSize(Vector2.zero);
			hoveringIndex = Mathf.Clamp(hoveringIndex, -1, currentNode.children.Count-1);

			///MAIN AREA
			mainRect = new Rect(rect.x, rect.y, rect.width, rect.height - helpRectHeight);
			GUI.Box(mainRect, "", (GUIStyle)"AnimationCurveEditorBackground");
			GUILayout.BeginArea(mainRect);

			//HEADER
			GUILayout.Space(5);
			GUI.color = new Color(1,1,1,0.7f);
			GUILayout.Label(string.Format("<size=20><b>{0}</b></size>", title), headerStyle);
			GUI.color = Color.white;



			///SEARCH
			if (e.keyCode == KeyCode.DownArrow){GUIUtility.keyboardControl = 0;}
			if (e.keyCode == KeyCode.UpArrow){GUIUtility.keyboardControl = 0;}
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUI.SetNextControlName("SearchToolbar");
			search = EditorGUILayout.TextField(search, (GUIStyle)"ToolbarSeachTextField");
			if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton")){
				search = string.Empty;
				GUIUtility.keyboardControl = 0;
			}
			GUILayout.EndHorizontal();
			EditorUtils.BoldSeparator();


			///BACK
			if (currentNode.parent != null && string.IsNullOrEmpty(search)){
				GUILayout.BeginHorizontal("box");
				if (GUILayout.Button(string.Format("<b><size=18>◄ {0}</size></b>", currentNode.path), (GUIStyle)"label" )){
					currentNode = currentNode.parent;
				}
				GUILayout.EndHorizontal();
				var lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(e.mousePosition)){
					GUI.DrawTexture(lastRect, EditorUtils.GetTexture(hoverColor));
					base.editorWindow.Repaint();
				}
			}


			///TREE
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
			GUILayout.BeginVertical();

			if (search != lastSearch){
				if (!string.IsNullOrEmpty(search) && search.Length >= 2 ){
					var searchRootNode = new Node();
					foreach (var item in items){
						var itemText = item.content.text.Split('/').Last();
						if ( itemText.ToUpper().Replace(" ", "").Contains( search.ToUpper().Replace(" ", "") )){
							var childNode = new Node();
							childNode.item = item;
							searchRootNode.children[itemText] = childNode;
						}
					}
					currentNode = searchRootNode;
				} else {
					currentNode = rootNode;
				}
				lastSearch = search;
			}



			var i = 0;
			foreach (var childPair in currentNode.children){

				var leafItem = childPair.Value.item;
				var icon = leafItem != null? leafItem.content.image : EditorUtils.folderIcon;

				GUILayout.BeginHorizontal("box");

				//Prefix icon
				GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(16));

				//Content
				if (GUILayout.Button(string.Format("<size=12>{0}</size>",
					(leafItem == null? string.Format("<b>{0}</b>", childPair.Key) : childPair.Key) ),
					(GUIStyle)"label", GUILayout.Width(0), GUILayout.ExpandWidth(true) ))
				{

					if (leafItem != null){
						
						ExecuteItemFunc(leafItem);
						break;

					} else {

						currentNode = childPair.Value;
						hoveringIndex = 0;
						break;
					}
				}

				//Suffix icon
				GUILayout.Label(leafItem != null? "<b>+</b>" : "►", GUILayout.Width(20));

				GUILayout.EndHorizontal();
				var lastRect = GUILayoutUtility.GetLastRect();

				if (lastRect.Contains(e.mousePosition)){
					hoveringIndex = i;
				}

				if (hoveringIndex == i){
					GUI.DrawTexture(lastRect, EditorUtils.GetTexture(hoverColor));
					base.editorWindow.Repaint();
				}

				i++;
			}

			//handle the events
			HandeEvents(e);


			if (!init){
				init = true;
				EditorGUI.FocusTextInControl("SearchToolbar");
			}

			GUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();


			GUILayout.FlexibleSpace();


			///HELP AREA
			var hoveringNode = hoveringIndex >= 0? currentNode.children.Values.ToList()[hoveringIndex] : null;
			var helpRect = new Rect(rect.x + 2, rect.yMax-helpRectHeight + 2, rect.width - 4, helpRectHeight - 2);
			GUI.color = new Color(0,0,0,0.3f);
			GUI.Box(helpRect, "", (GUIStyle)"TextField");
			GUI.color = Color.white;
			GUILayout.BeginArea(helpRect);
			GUILayout.BeginVertical();
			if (hoveringNode != null && hoveringNode.item != null){
				GUILayout.Label(hoveringNode.item.content.tooltip, EditorStyles.wordWrappedLabel);
			} else {
				GUILayout.Label("");
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();



			EditorGUIUtility.SetIconSize(Vector2.zero);
		}


		//Executes the item's registered delegate
		void ExecuteItemFunc(EditorUtils.MenuItemInfo item){
			if (item.func != null){
				item.func();
			} else {
				item.func2(item.userData);
			}
			base.editorWindow.Close();
		}


		//Handle events
		void HandeEvents(Event e){

			if (e.type == EventType.KeyDown){

				if (e.keyCode == KeyCode.RightArrow || e.keyCode == KeyCode.Return){
					var next = currentNode.children.Values.ToList()[hoveringIndex];
					if (e.keyCode == KeyCode.Return && next.item != null){
						ExecuteItemFunc(next.item);
					} else if (next.item == null){
						currentNode = next;
						hoveringIndex = 0;							
					}
				}
				
				if (e.keyCode == KeyCode.LeftArrow){
					var previous = currentNode.parent;
					if (previous != null){
						hoveringIndex = currentNode.parent.children.Values.ToList().IndexOf(currentNode);
						currentNode = previous;
					}
				}
				
				if (e.keyCode == KeyCode.DownArrow){
					hoveringIndex ++;
				}

				if (e.keyCode == KeyCode.UpArrow){
					hoveringIndex --;
				}

				if (e.keyCode == KeyCode.Escape){
					base.editorWindow.Close();
				}
			}

			if (e.isMouse && !mainRect.Contains(e.mousePosition)){
				hoveringIndex = -1;
			}			
		}
	}
}

#endif