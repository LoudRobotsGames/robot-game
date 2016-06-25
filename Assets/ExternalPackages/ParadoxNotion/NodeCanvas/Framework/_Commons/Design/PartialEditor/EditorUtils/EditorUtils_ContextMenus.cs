#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace ParadoxNotion.Design{

    /// <summary>
    /// ContextMenus, basicaly reflection ones
    /// </summary>

	partial class EditorUtils {

		///Get a selection menu of types deriving base type
		public static GenericMenu GetTypeSelectionMenu(Type baseType, Action<Type> callback, GenericMenu menu = null, string subCategory = null){

			if (menu == null)
				menu = new GenericMenu();

			if (subCategory != null)
				subCategory = subCategory + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object selectedType){
				callback((Type)selectedType);
			};

			var scriptInfos = GetScriptInfosOfType(baseType);

			foreach (var info in scriptInfos.Where(info => string.IsNullOrEmpty(info.category))) {
			    menu.AddItem(new GUIContent(subCategory + info.name, info.icon, info.description), false, info.type != null? Selected : null, info.type);
			}

			//menu.AddSeparator("/");

			foreach (var info in scriptInfos.Where(info => !string.IsNullOrEmpty(info.category))) {
			    menu.AddItem(new GUIContent(subCategory + info.category + "/" + info.name, info.icon, info.description), false, info.type != null? Selected : null, info.type);
			}

			return menu;
		}


		public static GenericMenu GetPreferedTypesSelectionMenu(Type type, Action<Type> callback, bool showInterfaces = true, GenericMenu menu = null, string subCategory = null){
			
			if (menu == null)
				menu = new GenericMenu();

			if (subCategory != null)
				subCategory = subCategory + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object t){
				callback((Type)t);
			};							

			var listTypes = new Dictionary<Type, string>();
			foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(object), false)){
				if (type.IsAssignableFrom(t) || (t.IsInterface && showInterfaces) ){
					var nsString = string.IsNullOrEmpty(t.Namespace)? "No Namespace/" : (t.Namespace.Replace(".","/") + "/") ;
					var finalString = nsString + t.FriendlyName();
					menu.AddItem(new GUIContent(subCategory + finalString), false, Selected, t);
					listTypes.Add( typeof(List<>).MakeGenericType(new Type[]{t}), finalString );
				}
			}

			//menu.AddSeparator(subCategory + "/");
			foreach(var tPair in listTypes){
				menu.AddItem(new GUIContent(subCategory + "List<T>/" + tPair.Value), false, Selected, tPair.Key);
			}

			menu.AddItem(new GUIContent(subCategory + "Add Type..."), false, ()=>{ PreferedTypesEditorWindow.ShowWindow(); });

			return menu;
		}

		//yeah this is very special but....
		public static void ShowPreferedTypesSelectionMenu(Type type, Action<Type> callback, bool showInterfaces = true){
			GetPreferedTypesSelectionMenu(type, callback, showInterfaces).ShowAsContext();
			Event.current.Use();
		}

		///Get a GenericMenu for field selection in a type
		public static GenericMenu GetFieldSelectionMenu(Type type, Type fieldType, Action<FieldInfo> callback, GenericMenu menu = null, string subMenu = null){
			
			if (menu == null)
				menu = new GenericMenu();

			if (subMenu != null)
				subMenu = subMenu + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object selectedField){
				callback((FieldInfo)selectedField);
			};

			var itemAdded = false;
			var more = false;
			foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(field => fieldType.IsAssignableFrom(field.FieldType))) {

				if (field.DeclaringType == typeof(MonoBehaviour))
					more = true;

				var category = more? subMenu + type.FriendlyName() + "/More" : subMenu + type.FriendlyName();

			    menu.AddItem(new GUIContent(string.Format("{0}/{1} : {2}", category, field.Name, field.FieldType.FriendlyName())), false, Selected, field);
			    itemAdded = true;
			}

			if (!itemAdded)
				menu.AddDisabledItem(new GUIContent(subMenu + type.FriendlyName()));

			return menu;
		}


		///Get a GenericMenu for properties of a type optionaly specifying mustRead & mustWrite
		public static GenericMenu GetPropertySelectionMenu(Type type, Type propType, Action<PropertyInfo> callback, bool mustRead = true, bool mustWrite = true, GenericMenu menu = null, string subMenu = null){
			
			if (menu == null)
				menu = new GenericMenu();

			if (subMenu != null)
				subMenu = subMenu + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object selectedProperty){
				callback((PropertyInfo)selectedProperty);
			};

			var itemAdded = false;
			var more = false;
			foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)){
				
				if (!prop.CanRead && mustRead)
					continue;

				if (!prop.CanWrite && mustWrite)
					continue;

				if (propType.IsAssignableFrom(prop.PropertyType)){

					if (prop.DeclaringType == typeof(MonoBehaviour))
						more = true;

					var category = more? subMenu + type.FriendlyName() + "/More" : subMenu + type.FriendlyName();

					menu.AddItem( new GUIContent( string.Format("{0}/{1} : {2}", category, prop.Name, prop.PropertyType.FriendlyName())), false, Selected, prop );
					itemAdded = true;
				}
			}

			if (!itemAdded)
				menu.AddDisabledItem(new GUIContent(subMenu + type.FriendlyName()));

			return menu;
		}

		///Get a GenericMenu for method or property get/set methods selection in a type
		public static GenericMenu GetMethodSelectionMenu(Type type, Type returnType, Type acceptedParamsType, System.Action<MethodInfo> callback, int maxParameters, bool propertiesOnly, bool excludeVoid = false, GenericMenu menu = null, string subMenu = null){

			if (menu == null)
				menu = new GenericMenu();

			if (subMenu != null)
				subMenu = subMenu + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object selectedMethod){
				callback((MethodInfo)selectedMethod);
			};

			var itemAdded = false;
			var unityInherited = false;
			foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)){

				if (propertiesOnly != method.IsSpecialName)
					continue;

				if (method.IsGenericMethod)
					continue;

				if (!returnType.IsAssignableFrom(method.ReturnType))
					continue;

				if (method.ReturnType == typeof(void) && excludeVoid)
					continue;

				var parameters = method.GetParameters();
				if (parameters.Length > maxParameters && maxParameters != -1)
					continue;

				if (parameters.Length > 0){
					if ( parameters.Any(param => !acceptedParamsType.IsAssignableFrom(param.ParameterType)) ) {
						continue;
					}
				}

				if (method.DeclaringType == typeof(MonoBehaviour))
					unityInherited = true;

				var category = unityInherited? subMenu + type.FriendlyName() + "/More" : subMenu + type.FriendlyName();

				menu.AddItem(new GUIContent( category + "/" + method.SignatureName()), false, Selected, method);
				itemAdded = true;
			}
			
			if (!itemAdded)
				menu.AddDisabledItem(new GUIContent(subMenu + type.FriendlyName()) );

			return menu;
		}

		///Get a menu for static methods in the list of types
		public static GenericMenu GetStaticMethodSelectionMenu(Type type, Action<MethodInfo> callback, GenericMenu menu = null, string subMenu = null){

			if (menu == null)
				menu = new GenericMenu();

			if (subMenu != null)
				subMenu = subMenu + "/";

			GenericMenu.MenuFunction2 Selected = delegate(object selectedMethod){
				callback((MethodInfo)selectedMethod);
			};			

			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
				menu.AddItem(new GUIContent(subMenu + type.Name + "/" + method.SignatureName()), false, Selected, method);

			return menu;
		}

		///Get a GenericMenu for Events of the type and only event handler type of System.Action
		public static GenericMenu GetEventSelectionMenu(Type type, Type argType, Action<EventInfo> callback, GenericMenu menu = null, string subMenu = null){

			if (menu == null)
				menu = new GenericMenu();
			
			if (subMenu != null)
				subMenu = subMenu + "/";
			
			GenericMenu.MenuFunction2 Selected = delegate(object selectedEvent){
				callback((EventInfo)selectedEvent);
			};
			
			var itemAdded = false;
			var eventType = argType == null? typeof(System.Action) : typeof(System.Action<>).MakeGenericType(new Type[]{argType});
			foreach (var e in type.GetEvents(BindingFlags.Instance | BindingFlags.Public)){
				if (e.EventHandlerType == eventType){
					var eventInfoString = string.Format("{0}({1})", e.Name, argType != null? argType.FriendlyName() : "");
					menu.AddItem(new GUIContent(subMenu + type.FriendlyName() + "/" + eventInfoString), false, Selected, e);
					itemAdded = true;					
				}
			}

			if (!itemAdded)
				menu.AddDisabledItem(new GUIContent(subMenu + type.FriendlyName()) );
			
			return menu;
		}

		///Get a GenericMenu for Static Events of the type and only event handler type of System.Action
		public static GenericMenu GetStaticEventSelectionMenu(Type type, Type argType, Action<EventInfo> callback, GenericMenu menu = null, string subMenu = null){

			if (menu == null)
				menu = new GenericMenu();
			
			if (subMenu != null)
				subMenu = subMenu + "/";
			
			GenericMenu.MenuFunction2 Selected = delegate(object selectedEvent){
				callback((EventInfo)selectedEvent);
			};
			
			var itemAdded = false;
			var eventType = argType == null? typeof(System.Action) : typeof(System.Action<>).MakeGenericType(new Type[]{argType});
			foreach (var e in type.GetEvents(BindingFlags.Static | BindingFlags.Public)){
				if (e.EventHandlerType == eventType){
					var eventInfoString = string.Format("{0}({1})", e.Name, argType != null? argType.FriendlyName() : "");
					menu.AddItem(new GUIContent(subMenu + type.FriendlyName() + "/" + eventInfoString), false, Selected, e);
					itemAdded = true;					
				}
			}

			if (!itemAdded)
				menu.AddDisabledItem(new GUIContent(subMenu + type.FriendlyName()) );
			
			return menu;
		}


		///Shows a GenericMenu for fields of all components of a game object
		public static void ShowGameObjectFieldSelectionMenu(GameObject go, Type fieldType, System.Action<FieldInfo> callback){
			var menu = new GenericMenu();
			foreach (var comp in go.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) )
				menu = GetFieldSelectionMenu(comp.GetType(), fieldType, callback, menu);
			menu.ShowAsContext();
			Event.current.Use();
		}

		///Shows a GenericMenu for properties of all components of a game object
		public static void ShowGameObjectPropertySelectionMenu(GameObject go, Type propType, Action<PropertyInfo> callback, bool mustRead = true, bool mustWrite = true){
			var menu = new GenericMenu();
			foreach (var comp in go.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) )
				menu = GetPropertySelectionMenu(comp.GetType(), propType, callback, mustRead, mustWrite, menu);
			menu.ShowAsContext();
			Event.current.Use();
		}

		///Shows a GenericMenu for methods of all components of a game object
		public static void ShowGameObjectMethodSelectionMenu(GameObject go, Type returnType, Type paramsType, System.Action<MethodInfo> callback, int maxParameters, bool propertiesOnly, bool excludeVoid = false){
			var menu = new GenericMenu();
			foreach (var comp in go.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) )
				menu = GetMethodSelectionMenu(comp.GetType(), returnType, paramsType, callback, maxParameters, propertiesOnly, excludeVoid, menu);
			menu.ShowAsContext();
			Event.current.Use();
		}


		///Show an Event selection menu for all components on a game object
		public static void ShowGameObjectEventSelectionMenu(GameObject go, Type argType, System.Action<EventInfo> callback){
			var menu = new GenericMenu();
			foreach(var comp in go.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) )
				menu = GetEventSelectionMenu(comp.GetType(), argType, callback, menu);
			menu.ShowAsContext();
			Event.current.Use();
		}



		//A generic menu selection
		public static void ShowMenu<T>(List<T> options, Action<T> callback){
			
			GenericMenu.MenuFunction2 Selected = delegate(object selection){
				callback((T)selection);
			};

			var menu = new GenericMenu();
			foreach (var element in options)
				menu.AddItem(new GUIContent(element.ToString()), false, Selected, element );
			menu.ShowAsContext();
			Event.current.Use();
		}


		public class MenuItemInfo{
			public GUIContent content;
			public GenericMenu.MenuFunction func;
			public GenericMenu.MenuFunction2 func2;
			public object userData;
			public MenuItemInfo(GUIContent c, GenericMenu.MenuFunction f1, GenericMenu.MenuFunction2 f2, object o){
				content = c;
				func = f1;
				func2 = f2;
				userData = o;
			}
		}

		///Gets an array of MenuItemInfo out of the GenericMenu provided
		public static MenuItemInfo[] GetMenuItems(GenericMenu menu){

			var result = new List<MenuItemInfo>();
			var field = typeof(GenericMenu).GetField("menuItems", BindingFlags.Instance | BindingFlags.NonPublic);
			var items = field.GetValue(menu) as ArrayList;

			foreach (var item in items){
				var content = item.GetType().GetField("content").GetValue(item) as GUIContent;
				var func1 = item.GetType().GetField("func").GetValue(item) as GenericMenu.MenuFunction;
				var func2 = item.GetType().GetField("func2").GetValue(item) as GenericMenu.MenuFunction2;
				var userData = item.GetType().GetField("userData").GetValue(item);
				result.Add( new MenuItemInfo( content, func1, func2, userData ) );
			}
			
			return result.ToArray();
		}

	}
}

#endif