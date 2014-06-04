using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PsdLayerRect
{
	#region static members
	
	public static PsdLayerRect zero
	{
		get { return new PsdLayerRect(); }
	}
	
	#endregion
	
	#region members
	
	public float left = 0;
	public float top = 0;
	public float right = 0;
	public float bottom = 0;
	public float width
	{
		get { return this.right - this.left; }
	}
	public float height
	{
		get { return this.bottom - this.top; }
	}
	
	public PsdLayerRect()
	{
	}
	public PsdLayerRect(float l, float t, float w, float h)
	{
		this.left = l;
		this.top = t;
		this.right = l + w;
		this.bottom = t + h;
	}
	
	#endregion
};

public class PsdLayerCommandParser
{
	// ControlType
	
	public enum ControlType
	{
		Script,
		
		Container,
		Panel,
		
		Sprite,
		SpriteFont,
		SpriteAnimation,
		Label,
		LabelButton,
		Input,
		Password,
		Button,
		Toggle,
		ComboBox,
		VScrollBar,
		HScrollBar,
		ScrollView,
		VirtualView,
		Texture
	};
	
	// ControlParser
	
	public class ControlParser
	{
		public enum Align
		{
			TopLeft,
			Top,
			TopRight,
			Left,
			Center,
			Right,
			BottomLeft,
			Bottom,
			BottomRight,
		};
		
		public string srcFileDirPath = "";
		public string srcFilePath{
			get{ return this.srcFileDirPath + '/' + this.fullName; }
		}
		public string originalName = "";
		public string fullName = "";
		public string name = "";
		public string command = "";
		public PsdLayerRect area;
		
		public ControlType type = ControlType.Sprite;
		public PsdLayerRect sliceArea;
		public bool hasBoxCollider = false;
		public Color color = Color.white;

		public int fontSize;
		public string text;
		public bool bold;
		public bool shadow;
		public Align align;

		public int fps;

		public ControlParser()
		{
			this.type = ControlType.Container;
		}
		public ControlParser(string srcFileDirPath, PsdLayerExtractor.Layer layer)
		{
			var errorPreMsg = "Parse error at '" +srcFileDirPath+ "/" +layer.name+ "'.";
			var name_cmdAndVals = layer.name.Split('@');
			
			this.srcFileDirPath = srcFileDirPath;
			this.originalName = layer.name;
			this.name = name_cmdAndVals[0];
			{
				for (var i=1; i<name_cmdAndVals.Length; ++i)
				{
					name_cmdAndVals[i] = name_cmdAndVals[i].Trim().ToLower();
				}
			}
			
			this.ParseCommandType(name_cmdAndVals, layer, errorPreMsg);
			if (this.type == ControlType.Script)
			{
				this.text = string.IsNullOrEmpty(layer.text) ? "" : layer.text.Trim();
			}
			else
			{
				this.fullName = this.name + 
					(string.IsNullOrEmpty(this.command) ? "" : '@' + this.command);
				
				var comment = this.ParseComment(name_cmdAndVals);
				if (!string.IsNullOrEmpty(comment))
					this.name = this.name + "(" + comment + ")";
				
				this.area = new PsdLayerRect(
					layer.psdLayer.area.left, 
					layer.psdLayer.area.top, 
					layer.psdLayer.area.width, 
					layer.psdLayer.area.height);
				
				this.hasBoxCollider = this.ParseCollider(name_cmdAndVals) == "box";
				this.sliceArea = this.ParseSliceArea(name_cmdAndVals);
				this.color = this.ParseColor(name_cmdAndVals);
				if (!string.IsNullOrEmpty(layer.text))
				{
					this.text = layer.text.Trim();
					var arr = this.text.Split('\n');
					this.fontSize = Mathf.FloorToInt(this.area.height / arr.Length * 0.92f);
				}
				else
					this.text = this.ParseText(name_cmdAndVals);
				
				this.bold = this.IsBold(name_cmdAndVals);
				this.shadow = this.IsShadow(name_cmdAndVals);
				this.align = this.ParseAlign(name_cmdAndVals);

				this.fps = this.ParseFps(name_cmdAndVals);
			}
		}
		
		private Color StringToColor(string clr)
		{
			switch (clr)
			{
			case "white": return Color.white;
			case "black": return Color.black;
			case "blue": return Color.blue;
			case "green": return Color.green;
			case "red": return Color.red;
			case "cyan": return Color.cyan;
			case "gray": return Color.gray;
			case "magenta": return Color.magenta;
			case "yellow": return Color.yellow;
			default:
				if (!string.IsNullOrEmpty(clr) && clr.Length >= 6)
				{
					var r = "";
					var g = "";
					var b = "";
					var ch = clr[0];
					
					var k = 0;
					if (ch == '#')
						k++;
						
					r += clr[k++];
					r += clr[k++];
					g += clr[k++];
					g += clr[k++];
					b += clr[k++];
					b += clr[k++];
					
					var rr = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
					var gg = int.Parse(g, System.Globalization.NumberStyles.HexNumber);
					var bb = int.Parse(b, System.Globalization.NumberStyles.HexNumber);
					return new Color(rr / 255f, gg / 255f, bb / 255f);
				}
				break;
			}
			return Color.white;
		}
		
		private bool IsBold(string[] name_cmdAndVals)
		{
			var s = this.ParseCommand("bold", name_cmdAndVals);
			return s == "true" || s == "1";
		}
		
		private bool IsShadow(string[] name_cmdAndVals)
		{
			var s = this.ParseCommand("shadow", name_cmdAndVals);
			return s == "true" || s == "1";
		}
		
		private string ParseComment(string[] name_cmdAndVals)
		{
			return this.ParseCommand("comment", name_cmdAndVals);
		}

		private string ParseText(string[] name_cmdAndVals)
		{
			return this.ParseCommand("text", name_cmdAndVals);
		}
		
		private Align ParseAlign(string[] name_cmdAndVals)
		{
			var align = this.ParseCommand("align", name_cmdAndVals);
			switch (align)
			{
			case "topleft": return Align.TopLeft;
			case "top": return Align.Top;
			case "topright": return Align.TopRight;
			case "middleleft": 
			case "left": return Align.Left;
			case "middle": 
			case "center": return Align.Center;
			case "middleright": 
			case "right": return Align.Right;
			case "bottomleft": return Align.BottomLeft;
			case "bottom": return Align.Bottom;
			case "bottomright": return Align.BottomRight;
			}
			return Align.Center;
		}
		
		private int ParseFps(string[] name_cmdAndVals)
		{
			var v = 0;
			if (int.TryParse(this.ParseCommand("fps", name_cmdAndVals), out v))
				return v;
			else
				return 30;
		}
		
		private Color ParseColor(string[] name_cmdAndVals)
		{
			return this.StringToColor(this.ParseCommand("color", name_cmdAndVals));
		}
		
		private string ParseCollider(string[] name_cmdAndVals)
		{
			return this.ParseCommand("collider", name_cmdAndVals);
		}
		
		private int ParseSliceValue(string v, bool vertical)
		{
			var isPercentage = v.Length > 0 ? (v[v.Length-1] == '%') : false;
			if (isPercentage)
			{
				v = v.Remove(v.Length-1).Trim();
				var per = float.Parse(v);
				if (per > 50)
					per = 50;
				
				if (vertical)
					return (int)((float)this.area.height * (per / 100f));
				else
					return (int)((float)this.area.width * (per / 100f));
			}
			else
			{
				var ret = int.Parse(v);
				return ret > 2 ? ret : 2;
			}
		}
		
		private PsdLayerRect ParseSliceArea(string[] name_cmdAndVals)
		{
			var area = this.ParseCommand("slice", name_cmdAndVals);
			if (!string.IsNullOrEmpty(area))
			{
				var values = area.Split('x');
				var rc = new PsdLayerRect();
				{
					rc.left = this.ParseSliceValue(values[0], false);
					rc.top = values.Length < 2 ? rc.left : this.ParseSliceValue(values[1], true);
					rc.right = values.Length < 3 ? rc.left : this.ParseSliceValue(values[2], false);
					rc.bottom = values.Length < 4 ? rc.top : this.ParseSliceValue(values[3], true);
				}
				return rc;
			}
			return null;
		}
		
		private string ParseCommand(string command, string[] name_cmdAndVals)
		{
			return this.ParseCommand(command, name_cmdAndVals, true);
		}
		private string ParseCommand(string command, string[] name_cmdAndVals, bool removeCommandName)
		{
			var reg = new Regex(@"\s*" + command + @"\s*=");
			for (var n=1; n<name_cmdAndVals.Length; ++n)
			{
				if (reg.IsMatch(name_cmdAndVals[n]))
				{
					if (removeCommandName)
						return reg.Replace(name_cmdAndVals[n], "").Trim();
					else
						return name_cmdAndVals[n];
				}
			}
			return null;
		}
		
		public void ParseCommandType(string[] name_cmdAndVals, 
			PsdLayerExtractor.Layer layer, string errorPreMsg)
		{
			this.command = "";
			this.type = ControlType.Sprite;
			
			for (var n=1; n<name_cmdAndVals.Length; ++n)
			{
				this.command = name_cmdAndVals[n];
				
				if (this.command == "script")
					this.type = ControlType.Script;
				
				else if (this.command == "panel")
					this.type = ControlType.Panel;
				
				else if (this.command == "scrollview" || this.command.StartsWith("scrollview."))
					this.type = ControlType.ScrollView;
					
				else if (this.command == "virtualview" || this.command.StartsWith("virtualview."))
					this.type = ControlType.VirtualView;
					
				else if (this.command == "button" || this.command.StartsWith("button.")){
					if (layer.isTextLayer)
						this.type = ControlType.LabelButton;
					else
						this.type = ControlType.Button;
				}
					
				else if (this.command == "checkbox" || this.command.StartsWith("checkbox.") || 
					this.command == "toggle" || this.command.StartsWith("toggle."))
					this.type = ControlType.Toggle;
					
				else if (this.command == "combobox" || this.command.StartsWith("combobox."))
					this.type = ControlType.ComboBox;
					
				else if (this.command == "input" || this.command.StartsWith("input.") ||
					this.command == "editbox" || this.command.StartsWith("editbox."))
					this.type = ControlType.Input;
					
				else if (this.command == "password" || this.command.StartsWith("password"))
					this.type = ControlType.Password;
				
				else if (this.command == "vscrollbar" || this.command.StartsWith("vscrollbar."))
					this.type = ControlType.VScrollBar;
					
				else if (this.command == "hscrollbar" || this.command.StartsWith("hscrollbar."))
					this.type = ControlType.HScrollBar;
					
				else if (this.command == "spritefont")
					this.type = ControlType.SpriteFont;
				
				else if (this.command == "animation" || this.command == "ani")
					this.type = ControlType.SpriteAnimation;

				else if (this.command == "texture")
					this.type = ControlType.Texture;
				
				if (this.type != ControlType.Sprite)
					return; // type setted
			}
			
			if (layer.isContainer)
			{
				this.type = ControlType.Container;
			}
			else if (layer.isTextLayer)
			{
				this.type = ControlType.Label;
			}
			else{
				if (!string.IsNullOrEmpty(this.command) &&
					!this.command.StartsWith("comment") &&
					!this.command.StartsWith("color") &&
					!this.command.StartsWith("text") &&
					!this.command.StartsWith("bold") &&
					!this.command.StartsWith("shadow") &&
					!this.command.StartsWith("collider") &&
					!this.command.StartsWith("slice") &&
					!this.command.StartsWith("align") &&
					!this.command.StartsWith("fps") &&
					!this.command.StartsWith("ignore"))
				{
					Debug.LogError(errorPreMsg + "'" + this.command + "'" + " is wrong command or attribute");
				}
				this.command = "sprite";
				this.type = ControlType.Sprite;
			}
		}
	} ;
	
	// Control
	
	public class Control
	{
		public ControlParser pa;
		public List<Control> sources = new List<Control>();
		public List<Control> children = new List<Control>();
		
		public ControlType type{
			get{ return this.pa.type; }
		}
		public string command{
			get{ return this.pa.command; }
		}
		public string fullName{
			get{ return this.pa.fullName; }
		}
		public string name{
			get{ return this.pa.name; }
		}
		public PsdLayerRect area{
			get{ return this.pa.area; }
		}
		public PsdLayerRect sliceArea{
			get{ return this.pa.sliceArea; }
		}
		public bool hasBoxCollider{
			get{ return this.pa.hasBoxCollider; }
		}
		public Color color{
			get{ return this.pa.color; }
		}

		public int fontSize{
			get{ return this.pa.fontSize; }
		}
		public string text{
			get{ return this.pa.text; }
		}
		public bool bold{
			get{ return this.pa.bold; }
		}
		public bool shadow{
			get{ return this.pa.shadow; }
		}
		public ControlParser.Align align{
			get{ return this.pa.align; }
		}

		public int fps{
			get{ return this.pa.fps; }
		}

		public Control()
		{
			this.pa = new ControlParser();
		}
		
		public Control(ControlParser pa)
		{
			this.pa = pa;
		}
	} ;
	
	// members
	
	public Control root = new Control();
	
	private Control ParseControlAndSources(ref int i, ref List<PsdLayerExtractor.Layer> layers, 
		ControlParser pa)
	{
		var layer = layers[i];
		if (!layer.canLoadLayer)
			return null;
		
		var control = null as Control;
		var list = new List<Control>();
		for (; i<layers.Count; ++i)
		{
			layer = layers[i];
			if (!layer.canLoadLayer)
				continue;
			
			pa = new ControlParser(pa.srcFileDirPath, layer);
			
			if (list.Count > 0 && list[0].name != pa.name)
			{
				i--;
				break;
			}
			
			var source = null as Control;
			if (layer.isContainer)
			{
				source = new Control(pa);
				this.ParseImple(source, pa.srcFileDirPath, layer.children);
			}
			else
			{
				source = new Control(pa);
			}
			
			if (list.Find(s => s.fullName == pa.fullName) != null)
			{
				i--;
				break;
			}
			list.Add(source);
		}
		
		if (list.Count > 0)
		{
			list.Sort((a, b) => string.Compare(a.command, b.command));
			control = list[0];
			control.sources = list;
		}
		return control;
	}
	
	private void ParseImple(Control container, string srcFileDirPath, List<PsdLayerExtractor.Layer> layers)
	{
		for ( var i=0; i<layers.Count; ++i)
		{
			var layer = layers[i];
			if (!layer.canLoadLayer)
				continue;

			var pa = new ControlParser(srcFileDirPath, layer);
			var control = this.ParseControlAndSources(ref i, ref layers, pa);
			if (control != null)
				container.children.Add(control);
		}
	}
	
	public void Parse(string srcFileDirPath, List<PsdLayerExtractor.Layer> layers)
	{
		this.ParseImple(this.root, srcFileDirPath, layers);
	}
};