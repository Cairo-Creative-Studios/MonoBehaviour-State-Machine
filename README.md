# MonoBehaviour-State-Machine
A State Machine for Unity Game Objects that Enables/Disables sets of MonoBehaviours depending on the state the Object is in.

# Installation
Download the Release zip and export into your scripts folder. 

# Use
Add the Component called MB State Machine on any Game Object, then get a reference to it in any script.
Call stateMachine.SetState("New State") to change the state.
The Inspector of the MB State Machine allows you to add States, set their name, and add MonoBehaviours to the State from a Drop Down List. 
The MB State Machine also fires Unity Events that can be used for extended customization.
