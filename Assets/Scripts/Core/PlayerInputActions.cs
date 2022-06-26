//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/Scripts/Core/PlayerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace KefirTest.Core
{
    public partial class @PlayerInputActions : IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""ShipControls"",
            ""id"": ""be28e408-3a74-42ee-82f2-1c43a2f8ef84"",
            ""actions"": [
                {
                    ""name"": ""Rotation"",
                    ""type"": ""Value"",
                    ""id"": ""95edc2ca-b99f-444e-a5ff-9ffe52263b62"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Acceleration"",
                    ""type"": ""Value"",
                    ""id"": ""b7a80780-f724-4bd9-8dd4-507c0ef22084"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""PrimaryFire"",
                    ""type"": ""Value"",
                    ""id"": ""e0d71504-b740-49d2-953e-8dd5439c664a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SecondaryFire"",
                    ""type"": ""Button"",
                    ""id"": ""75366830-b4b1-442b-9ffb-23b597a57a46"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Arrows"",
                    ""id"": ""422070ab-874d-4743-9d40-2f70a5a613e8"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""f05c6f81-8028-472c-b2bb-219320057cdb"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""575799f4-ce41-4b70-a711-25123e84a2af"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""AD"",
                    ""id"": ""cca3ffdb-ec92-4bdb-a3ba-a277b8294248"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""ffffe2e5-a1cb-4f4d-9da5-70a62eb69755"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""edf8646d-0a61-4c17-8630-681822f3e43b"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""4258e7bc-5682-440e-a034-fabc9d12c380"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Acceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b21cef18-eab1-4441-a4e8-54cd432cf523"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Acceleration"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6a2b6996-55da-4013-a35a-3123ca005ce6"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f468e058-faeb-4250-88fd-9ab0e5d733d1"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""def43295-510f-4f96-a829-489556ae1676"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d053f87d-0aae-4738-9e91-5610e179be66"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // ShipControls
            m_ShipControls = asset.FindActionMap("ShipControls", throwIfNotFound: true);
            m_ShipControls_Rotation = m_ShipControls.FindAction("Rotation", throwIfNotFound: true);
            m_ShipControls_Acceleration = m_ShipControls.FindAction("Acceleration", throwIfNotFound: true);
            m_ShipControls_PrimaryFire = m_ShipControls.FindAction("PrimaryFire", throwIfNotFound: true);
            m_ShipControls_SecondaryFire = m_ShipControls.FindAction("SecondaryFire", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }
        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }
        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // ShipControls
        private readonly InputActionMap m_ShipControls;
        private IShipControlsActions m_ShipControlsActionsCallbackInterface;
        private readonly InputAction m_ShipControls_Rotation;
        private readonly InputAction m_ShipControls_Acceleration;
        private readonly InputAction m_ShipControls_PrimaryFire;
        private readonly InputAction m_ShipControls_SecondaryFire;
        public struct ShipControlsActions
        {
            private @PlayerInputActions m_Wrapper;
            public ShipControlsActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Rotation => m_Wrapper.m_ShipControls_Rotation;
            public InputAction @Acceleration => m_Wrapper.m_ShipControls_Acceleration;
            public InputAction @PrimaryFire => m_Wrapper.m_ShipControls_PrimaryFire;
            public InputAction @SecondaryFire => m_Wrapper.m_ShipControls_SecondaryFire;
            public InputActionMap Get() { return m_Wrapper.m_ShipControls; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(ShipControlsActions set) { return set.Get(); }
            public void SetCallbacks(IShipControlsActions instance)
            {
                if (m_Wrapper.m_ShipControlsActionsCallbackInterface != null)
                {
                    @Rotation.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnRotation;
                    @Rotation.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnRotation;
                    @Rotation.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnRotation;
                    @Acceleration.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAcceleration;
                    @Acceleration.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAcceleration;
                    @Acceleration.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAcceleration;
                    @PrimaryFire.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnPrimaryFire;
                    @PrimaryFire.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnPrimaryFire;
                    @PrimaryFire.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnPrimaryFire;
                    @SecondaryFire.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSecondaryFire;
                    @SecondaryFire.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSecondaryFire;
                    @SecondaryFire.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSecondaryFire;
                }
                m_Wrapper.m_ShipControlsActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Rotation.started += instance.OnRotation;
                    @Rotation.performed += instance.OnRotation;
                    @Rotation.canceled += instance.OnRotation;
                    @Acceleration.started += instance.OnAcceleration;
                    @Acceleration.performed += instance.OnAcceleration;
                    @Acceleration.canceled += instance.OnAcceleration;
                    @PrimaryFire.started += instance.OnPrimaryFire;
                    @PrimaryFire.performed += instance.OnPrimaryFire;
                    @PrimaryFire.canceled += instance.OnPrimaryFire;
                    @SecondaryFire.started += instance.OnSecondaryFire;
                    @SecondaryFire.performed += instance.OnSecondaryFire;
                    @SecondaryFire.canceled += instance.OnSecondaryFire;
                }
            }
        }
        public ShipControlsActions @ShipControls => new ShipControlsActions(this);
        public interface IShipControlsActions
        {
            void OnRotation(InputAction.CallbackContext context);
            void OnAcceleration(InputAction.CallbackContext context);
            void OnPrimaryFire(InputAction.CallbackContext context);
            void OnSecondaryFire(InputAction.CallbackContext context);
        }
    }
}
