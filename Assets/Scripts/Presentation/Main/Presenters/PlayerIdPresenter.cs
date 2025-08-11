using TMPro;
using UnityEngine;

namespace Presentation.Main.Presenters
{
    public class PlayerIdPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public void Init(string value)
        {
            labelText.text = value;
        }
    }
}