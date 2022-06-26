using UnityEngine;
using TMPro;
using KefirTest.Core;

namespace KefirTest.Game
{
    //Просто выводит на экран нужную информацию в ответ на получаемые ивенты.
    public class GameInterface : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI coordinatesText;
        [SerializeField]
        private TextMeshProUGUI rotationAngleText;
        [SerializeField]
        private TextMeshProUGUI speedText;
        [SerializeField]
        private TextMeshProUGUI laserChargesCountText;
        [SerializeField]
        private TextMeshProUGUI laserChargesCooldownText;
        [SerializeField]
        private TextMeshProUGUI resultText;
        [SerializeField]
        private GameObject restartButton;

        public delegate void RestartGameHandle();
        public event RestartGameHandle RestartGameEvent;

        public void Initialize(SpaceEntitiesController spaceEntitiesController)
        {
            spaceEntitiesController.UpdatePlayerInformationEvent += OnUpdatePlayerInformationEvent;
            spaceEntitiesController.PlayerDestroyEvent += OnPlayerDestroyEvent;
            resultText.enabled = false;
            restartButton.SetActive(false);
        }

        private void OnPlayerDestroyEvent(int score)
        {
            resultText.enabled = true;
            resultText.text = "YOUR SCORE: " + score;
            restartButton.SetActive(true);
        }

        private void OnUpdatePlayerInformationEvent(Vector3 position, float rotation, float speed, int secondaryWeaponAmmoCount, float secondaryWeaponAmmoRestoreTime)
        {
            coordinatesText.text = "Position: " + position.x.ToString("F1") + " ; " + position.y.ToString("F1");
            rotationAngleText.text = "Rotation: "+rotation.ToString("F1");
            speedText.text = "Speed: "+speed.ToString("F1");
            laserChargesCountText.text ="Laser charges: "+ secondaryWeaponAmmoCount.ToString();
            laserChargesCooldownText.text ="Laser restore: "+ secondaryWeaponAmmoRestoreTime.ToString("F1");
        }

        public void OnRestartButtonClick()
        {
            RestartGameEvent?.Invoke();
        }
    }
}