using NodeCanvas.Framework;
using UnityEngine;


namespace NodeCanvas.DialogueTrees{

	///An interface to use for whats being said by a dialogue actor
	public interface IStatement{
		string text{get;}
		AudioClip audio{get;}
		string meta{get;}
	}

	///Holds data of what's being said usualy by an actor
	[System.Serializable]
	public class Statement : IStatement{

		[SerializeField]
		private string _text = string.Empty;
		[SerializeField]
		private AudioClip _audio;
		[SerializeField]
		private string _meta = string.Empty;

		public string text{
			get {return _text;}
			set {_text = value;}
		}

		public AudioClip audio{
			get {return _audio;}
			set {_audio = value;}
		}

		public string meta{
			get {return _meta;}
			set {_meta = value;}
		}

		//required
		public Statement(){}
		public Statement(string text){
			this.text = text;
		}

		public Statement(string text, AudioClip audio){
			this.text = text;
			this.audio = audio;
		}

		public Statement(string text, AudioClip audio, string meta){
			this.text = text;
			this.audio = audio;
			this.meta = meta;
		}

		///Replace the text of the statement found in brackets, with blackboard variables ToString
		public Statement BlackboardReplace(IBlackboard bb){
			var s = text;
			var i = 0;
			while ( (i = s.IndexOf('[', i)) != -1){
				
				var end = s.Substring(i + 1).IndexOf(']');
				var varName = s.Substring(i + 1, end);
				var output = s.Substring(i, end + 2);
				
				object o = null;
				if (bb != null){
					var v = bb.GetVariable(varName, typeof(object));
					if (v != null){
						o = v.value;
					}
				}
				s = s.Replace(output, o != null? o.ToString() : output);

				i++;
			}

			return new Statement(s, audio, meta);
		}

		public override string ToString(){
			return text;
		}
	}
}