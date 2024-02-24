# Pulni's Editor Tools for Unity

## Features
- TypePicker - an attribute for SerializeReference fields of abstract/interface types which allows picking which concrete types to use
  - ManagedReferenceFixerWindow - a window solving a common issue related to `[SerializeReference]` fields
- ExposedProperties - a component that visualizes/groups properties of other components
- DebugHelper - a set of menu items and APIs that allow you to debug specific object instances

## System Requirements
Unity **2021.3** or later.

## Installation
1. The package is available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.pulni.editor-tools
```
2. You can also install via git url by adding this entry in your **manifest.json**
```
"com.pulni.editor-tools": "https://github.com/pulni4kiya/EditorTools.git"
```

# Feature details
## TypePicker
TypePicker is an attribute plus a property drawer which in combination with the SerializeReference attribute allows you to serialize interfaces and abstract classes and choose which concrete instance you want to serialize and make all of that editable in the inspector.

### How to use
// Put code here Pulni

### Additional info
1. You can specify the display name in the picker menu, as well as the order of the elements, by addinf the `[TypePickerInfo("ClassDisplayName", 5)]` attribute to each class.
2. You can implement a custom instance function in the class containing the serialized references that filters the types in whatever way you want (perhaps only some classes are valid in some configuration?).

### Problems you'll likely encounter and their solutions
1. Changing the name of a class, namespace or assembly that is serialized using `[SerializeReference]` will cause serialization problems, but these can be solved by using the `[MovedFrom]` attribute (from the `UnityEngine.Scripting.APIUpdating` namespace) and specifying the old name.
2. Every now and then you may get an error like
   >Trying to update the managed reference registry with invalid propertyPath(likely caused by a missing reference instance)'managedReferences[X].Y', with value 'Z'
   which happens because data is serialized (usually as prefab override) but the serialized reference it's supposed to override no longer exists.
   This can be resolved by using the `ManagedReferenceFixerWindow`.

### ManagedReferenceFixerWindow
This is a window that basically cleans up old and unused `[SerializeReference]` data.
Using the window below, you can find all instances of such issues and clear them with a press of a button.

## ExposedProperties
ExposedProperties ia s simple component that just shows properties from other components, allowing you to show the most relevant properties of a hierarchy at its root, making it easier to set things up.

You can also customize the properties to show using a custom more descriptive name instead of their original, and you can make them read-only or not drawing their sub-properties.

### How to use

## DebugHelper
The DebugHelper allows you to debug specific instances of your objects and components - very useful when you have many instances of the same component, but you only need to debug one.

### How to use
The DebugHelper adds a "Toggle Debug" menu item that allow you to toggle debugging on any game object or component.
As soon as you click that menu item, you'll get a log whether the component was added or removed.
Once you've enabled debugging for a component, you can put conditional breakpoints inside its code with the `DBG.Check(this)` as condition.
Alternatively, you can use `DBG.LogIfDebugged`,  `DBG.BreakIfDebugged` and `DBG.ActionIfDebugged` to log stuff, trigger a debugger break or do something custom if a component is being debugged. These calls will get removed from builds (using the Conditional attribute)
Note: If the name DBG in the global namespace is nto convenient for you, you can add the `PULNI_NO_DBG` compilation symbol, and it won't exist, and instead you can use the class `DebugHelper` (in Pulni.EditorTools namespace) which provides the actual functionality.
