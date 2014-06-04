using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
public class PsdLayerSpriteAnimation : MonoBehaviour
{
	public int fps = 30;
	public bool loop = true;
	public string[] spriteNames;
	
	private UISprite sprite;
	private int currFrame;
	private GBlue.Timer timer = new GBlue.Timer();

	public bool IsPlaying{
		get { return this.timer.IsPlaying; }
	}

	void Start () {
		this.sprite = this.GetComponent<UISprite>();
		
		this.timer.Play(this, 1f / (float)this.fps, delegate(){
			this.timer.Reset();

			if (this.currFrame >= this.spriteNames.Length){
				this.currFrame = this.loop ? 0 : this.spriteNames.Length-1;
			}
			this.sprite.spriteName = this.spriteNames[this.currFrame++];
		});
	}

	void Update ()
	{
		this.timer.Interval = 1f / (float)this.fps;
		this.timer.Update();
	}

	public void SetSprites(string[] names)
	{
		this.spriteNames = names;
		
		if (this.sprite == null){
			this.sprite = this.GetComponent<UISprite>();
		}
		if (this.sprite != null && this.spriteNames.Length > 0){
			this.sprite.spriteName = this.spriteNames[this.currFrame = 0];
			this.sprite.MakePixelPerfect();
		}
	}
}
