using System.Collections.Generic;
using UnityEngine;

public class ArmyButtonManager : MonoBehaviour
{
    public List<SelectableButtonUI> buttons;

    private SelectableButtonUI currentSelected;

    public void SelectButton(SelectableButtonUI selected)
    {
        if (currentSelected != null)
        {
            currentSelected.SetSelected(false);
        }

        currentSelected = selected;
        currentSelected.SetSelected(true);
    }
}
