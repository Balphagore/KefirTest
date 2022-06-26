using UnityEngine;

namespace KefirTest.Core
{
    //Обработчик ввода пользователя. Использует скрипт, сгенерированный Input System.
    public class PlayerInput
    {
        private PlayerInputActions playerInputActions;
        private IManager manager;

        public delegate void PlayerRotateHandle(float rotation);
        public event PlayerRotateHandle PlayerRotateEvent;

        public delegate void PlayerAccelerateHandle(float acceleration);
        public event PlayerAccelerateHandle PlayerAccelerateEvent;

        public delegate void PlayerPrimaryFireHandle();
        public event PlayerPrimaryFireHandle PlayerPrimaryFireEvent;

        public delegate void PlayerSecondaryFireHandle();
        public event PlayerSecondaryFireHandle PlayerSecondaryFireEvent;


        public PlayerInput(IManager manager)
        {
            this.manager = manager;
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
        }

        public void Initialize()
        {
            manager.UpdateEvent += OnUpdateEvent;
        }

        private void OnUpdateEvent()
        {
            float rotationValue = playerInputActions.ShipControls.Rotation.ReadValue<float>();
            if (rotationValue != 0)
            {
                PlayerRotateEvent?.Invoke(rotationValue * Time.deltaTime);
            }
            float acceleration = playerInputActions.ShipControls.Acceleration.ReadValue<float>();
            if (acceleration != 0)
            {
                PlayerAccelerateEvent?.Invoke(acceleration * Time.deltaTime);
            }
            float primaryFire = playerInputActions.ShipControls.PrimaryFire.ReadValue<float>();
            if (primaryFire != 0)
            {
                PlayerPrimaryFireEvent?.Invoke();
            }
            float secondaryFire=playerInputActions.ShipControls.SecondaryFire.ReadValue<float>();
            if(secondaryFire != 0)
            {
                PlayerSecondaryFireEvent?.Invoke();
            }
        }
    }
}