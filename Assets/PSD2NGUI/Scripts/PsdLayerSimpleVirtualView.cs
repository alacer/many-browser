using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GBlue;

/**
 * PsdLayerSimpleVirtualViewSlot
 * This class is implemented for an example
 */
public class PsdLayerSimpleVirtualViewSlot
{
	public UILabel label;
	
	public PsdLayerSimpleVirtualViewSlot(GameObject go){
		this.label = GBlue.Util.FindComponent<UILabel>(go);
		if (this.label != null)
			this.label.color = Color.black;
	}
	
	public void Update(int itemIndex){
		if (this.label != null)
			this.label.text = itemIndex.ToString();
	}
};

/**
 * PsdLayerSimpleVirtualView
 * This class is implemented for an example
 */
public class PsdLayerSimpleVirtualView : MonoBehaviour {
	
	void Start () {
		var view = this.GetComponent<PsdLayerVirtualView>();
		{
			for (var i=0; i<1000; ++i){
				view.AddItem(null);
			}
		}
		view.bgPadding = new RectOffset(14, 14, 14, 14);

		view.OnInitSlot = delegate(GameObject go) {
			return new PsdLayerSimpleVirtualViewSlot(go);
		};
		view.OnUpdateSlot = delegate(object slot, object item, int slotIndex, int itemIndex) {
			var s = slot as PsdLayerSimpleVirtualViewSlot;
			s.Update(itemIndex);
		};
	}
}