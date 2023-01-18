using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UDT.FlowControl
{
    /// <summary>
    /// The Component State Machine allows Components/MonoBehaviours attached to the Game Object to be subscribed to States, and the State Machine automatically enables/disables the events based on the Object's current State.
    /// </summary>
    [AddComponentMenu("State Machines/MB State Machine")]
    [ExecuteAlways]
    public class MonoBehaviourStateMachine : MonoBehaviour
    {
        /// <summary>
        /// The currently active State
        /// </summary>
        [Tooltip("The currently active State")]
        public string state = "Default";
        /// <summary>
        /// The States of the Game Object
        /// </summary>
        [Tooltip("The States of the Game Object")]
        [SerializeField]
        public SerializableDictionary<string, State> states = new SerializableDictionary<string, State>();
        // The current state
        private State _currentState;
        private string _previousState;
        
        public delegate void StateChangedDelegate(string newState);
        public event StateChangedDelegate OnStateChange;

        private MonoBehaviourStateMachine[] _childMachines;
        private MonoBehaviourStateMachine _parentMachine;

        private void Start()
        {
            _childMachines = GetComponentsInChildren<MonoBehaviourStateMachine>();
            _parentMachine = GetComponentInParent<MonoBehaviourStateMachine>();
            if (_parentMachine == this)
                _parentMachine = null;
        }

        void Update()
        {
            foreach (string key in states.Keys)
            {
                foreach (StateComponent stateComponent in states[key].Components)
                {
                    stateComponent.gameObject = gameObject;
                    //Add this to the State Name List
                    if(!states[key].ComponentNames.Contains(stateComponent.monoBehaviour.GetType().Name))
                        states[key].ComponentNames.Add(stateComponent.monoBehaviour.GetType().Name);
                    //Enable the MonoBehaviour if this is the current State, or if the MonoBehaviour exists in the current state
                    stateComponent.monoBehaviour.enabled = (key == state)||states[state].ComponentNames.Contains(stateComponent.monoBehaviour.GetType().Name);
                }
            }
            
            // Update the current state
            _currentState?.UpdateState();
            
            //Add Parent States
            if(_parentMachine != null)
                foreach(string stateName in _parentMachine.states.Keys)
                    if(!states.ContainsKey(stateName))
                        states.Add(stateName, _parentMachine.states[stateName]);
            if(_childMachines.Length > 0)
                foreach(MonoBehaviourStateMachine child in _childMachines)
                    foreach(string stateName in states.Keys)
                        if(!child.states.ContainsKey(stateName) && child != this)
                            child.states.Add(stateName, states[stateName]);
        }

        /// <summary>
        /// Sets the current state of the Game Object
        /// </summary>
        /// <returns><c>true</c>, if state was set, <c>false</c> otherwise.</returns>
        /// <param name="state">State.</param>
        public bool SetState(string state)
        {
            //Set the Parent's state, if Set State was called from a child
            _parentMachine?.SetState(state);

            //Set the Child's state, if Set State was called from a parent
            foreach (MonoBehaviourStateMachine childMachine in _childMachines)
                if(childMachine != this)
                    childMachine.SetState(state);

            bool set = false;

            if (states.ContainsKey(state))
            {
                _previousState = this.state;
                
                _currentState?.ExitState();
                
                // Set the new state
                _currentState = states[state];
                
                // Enter the new state
                _currentState?.EnterState();
                
                foreach (StateComponent stateComponent in states[state].Components)
                {
                    stateComponent.monoBehaviour.enabled = false;
                }

                this.state = state;
                set = true;

                foreach (StateComponent stateComponent in states[state].Components)
                {
                    stateComponent.monoBehaviour.enabled = true;
                }
                
                OnStateChange?.Invoke(state);
            }
            
            return set;
        }

        public void RevertState()
        {
            if (_previousState != null)
            {
                SetState(_previousState);
            }
        }
    }

    [Serializable]
    public class StateComponent
    {
        /// <summary>
        /// The Game Object this is attached to
        /// </summary>
        [Tooltip("The Game Object this is attached to")]
        public GameObject gameObject;
        /// <summary>
        /// The Component for the Item
        /// </summary>
        [Tooltip("The Component for the Item")]
        [Dropdown("GetMonoBehaviours")]
        public MonoBehaviour monoBehaviour;

        DropdownList<MonoBehaviour> GetMonoBehaviours()
        {
            DropdownList<MonoBehaviour> returnValue = new DropdownList<MonoBehaviour>();
            returnValue.Add("Null", null);

            MonoBehaviour[] monoBehaviours = gameObject.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour instance in monoBehaviours)
            {
                returnValue.Add(instance.GetType().Name, instance);
            }

            return returnValue;
        }
    }
    
    [Serializable]
    // A class representing a state in the state machine
    public class State
    {
        public List<string> ComponentNames = new List<string>(); 
        public List<StateComponent> Components = new List<StateComponent>();
        
        public UnityEvent onEnter;
        public UnityEvent onUpdate;
        public UnityEvent onExit;

        public void EnterState()
        {
            onEnter.Invoke();
        }

        public void UpdateState()
        {
            onUpdate.Invoke();
        }

        public void ExitState()
        {
            onExit.Invoke();
        }
    }
}
