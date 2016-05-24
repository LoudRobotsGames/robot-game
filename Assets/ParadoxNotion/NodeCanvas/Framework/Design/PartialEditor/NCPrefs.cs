#if UNITY_EDITOR

using UnityEditor;


namespace NodeCanvas.Editor{

	///Holds NC preferences
	public static class NCPrefs {

		static bool loaded = false;
		static bool _showNodeInfo;
		static bool _isLocked;
		static bool _iconMode;
		static int _curveMode;
		static bool _doSnap;
		static bool _showTaskSummary;
		static bool _showBlackboard;
		static bool _showNodePanel;
		static bool _showComments;
		static bool _hierarchicalMove;
		static bool _useExternalInspector;
	    static bool _showWelcomeWindow;
	    static bool _logEvents;

		public static bool showNodeInfo{
			get {if (!loaded) Load(); return _showNodeInfo;}
			set {_showNodeInfo = value; Save();}
		}

		public static bool isLocked{
			get {if (!loaded) Load(); return _isLocked;}
			set {if (_isLocked != value){ _isLocked = value; Save();} }
		}

		public static bool iconMode{
			get {if (!loaded) Load(); return _iconMode;}
			set {_iconMode = value; Save();}
		}

		public static int curveMode{
			get {if (!loaded) Load(); return _curveMode;}
			set {_curveMode = value; Save();}
		}
		
		public static bool doSnap{
			get {if (!loaded) Load(); return _doSnap;}
			set {_doSnap = value; Save();}
		}

		public static bool showTaskSummary{
			get {if (!loaded) Load(); return _showTaskSummary;}
			set {_showTaskSummary = value; Save();}
		}

		public static bool showBlackboard{
			get {if (!loaded) Load(); return _showBlackboard;}
			set {_showBlackboard = value; Save();}
		}

		public static bool showNodePanel{
			get {if (!loaded) Load(); return _showNodePanel;}
			set {_showNodePanel = value; Save();}
		}

		public static bool showComments{
			get {if (!loaded) Load(); return _showComments;}
			set {_showComments = value; Save();}			
		}

		public static bool hierarchicalMove{
			get {if (!loaded) Load(); return _hierarchicalMove;}
			set {_hierarchicalMove = value; Save();}			
		}

		public static bool useExternalInspector{
			get {if (!loaded) Load(); return _useExternalInspector;}
			set {_useExternalInspector = value; Save();}			
		}

	    public static bool showWelcomeWindow {
	        get {if ( !loaded ) Load(); return _showWelcomeWindow;}
            set {_showWelcomeWindow = value; Save();}
	    }

	    public static bool logEvents {
	        get {if ( !loaded ) Load(); return _logEvents;}
            set {_logEvents = value; Save();}
	    }

		//Save the prefs
		static void Save(){
			EditorPrefs.SetBool("NC.NodeInfo", _showNodeInfo);
			EditorPrefs.SetBool("NC.IsLocked", _isLocked);
			EditorPrefs.SetBool("NC.IconMode", _iconMode);
			EditorPrefs.SetInt("NC.CurveMode", _curveMode);
			EditorPrefs.SetBool("NC.DoSnap", _doSnap);
			EditorPrefs.SetBool("NC.TaskSummary", _showTaskSummary);
			EditorPrefs.SetBool("NC.ShowBlackboard", _showBlackboard);
			EditorPrefs.SetBool("NC.ShowNodePanel", _showNodePanel);
			EditorPrefs.SetBool("NC.ShowComments", _showComments);
			EditorPrefs.SetBool("NC.HierarchicalMove", _hierarchicalMove);
			EditorPrefs.SetBool("NC.UseExternalInspector", _useExternalInspector);
            EditorPrefs.SetBool("NC.ShowWelcomeWindow", _showWelcomeWindow);
            EditorPrefs.SetBool("NC.LogEvents", _logEvents);
		}

		//Load the prefs
		static void Load(){
			_showNodeInfo         = EditorPrefs.GetBool("NC.NodeInfo", true);
			_isLocked             = EditorPrefs.GetBool("NC.IsLocked", false);
			_iconMode             = EditorPrefs.GetBool("NC.IconMode", true);
			_curveMode            = EditorPrefs.GetInt("NC.CurveMode", 0);
			_doSnap               = EditorPrefs.GetBool("NC.DoSnap", true);
			_showTaskSummary      = EditorPrefs.GetBool("NC.TaskSummary", true);
			_showBlackboard       = EditorPrefs.GetBool("NC.ShowBlackboard", true);
			_showNodePanel        = EditorPrefs.GetBool("NC.ShowNodePanel", true);
			_showComments         = EditorPrefs.GetBool("NC.ShowComments", true);
			_hierarchicalMove     = EditorPrefs.GetBool("NC.HierarchicalMove", false);
			_useExternalInspector = EditorPrefs.GetBool("NC.UseExternalInspector", false);
		    _showWelcomeWindow    = EditorPrefs.GetBool("NC.ShowWelcomeWindow", true);
		    _logEvents   		  = EditorPrefs.GetBool("NC.LogEvents", true);
            loaded                = true;
		}
	}
}

#endif