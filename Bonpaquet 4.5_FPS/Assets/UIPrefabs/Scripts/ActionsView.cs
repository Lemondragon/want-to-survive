using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionsView : MonoBehaviour,IObserver {
	[SerializeField]
	private QuickSlotButton[] m_QuickSlotButtons;
	[SerializeField]
	private Image m_ActionCompletionBar;
	[SerializeField]
	private GameObject m_ActionCompletionObject;
	[SerializeField]
	private Text m_ActionName;
	[SerializeField]
	private Sprite m_EmptyQuickSlot;

	private void updateQuickSlot(int p_Number)
	{
		Button btn = m_QuickSlotButtons[p_Number].GetComponent<Button> ();
		Image img = btn.GetComponentsInChildren<Image> ()[1];
		img.sprite = PlayerUI.m_QuickSlotManager.getQuickSlotIcon (p_Number);
		if(img.sprite==null)
		{
			btn.interactable = true;
			img.sprite = this.m_EmptyQuickSlot;
		}
		else
		{
			btn.interactable = true;
		}
	}

	public void update(ObserverMessages p_Message, object p_Argument)
	{
		switch(p_Message)
		{
		case ObserverMessages.QuickSlotUpdated:
			if(p_Argument is int)
			{
				this.updateQuickSlot((int)p_Argument);
			}
			break;
		case ObserverMessages.ActionCompletionChanged:
			float amount = (float)p_Argument;
			if(amount>=0)
			{
				this.m_ActionCompletionObject.SetActive(true);
				this.m_ActionCompletionBar.materialForRendering.SetFloat("_CutOff", Mathf.Lerp(1, 0.02f,amount/100));
			}
			else
			{
				this.m_ActionCompletionObject.SetActive(false);
			}
			break;
		case ObserverMessages.ActionNameChange:
			this.m_ActionName.text=(string)p_Argument;
			break;
		}
	}
}
