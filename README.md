# Pulni's Editor Tools for Unity
\- *You shouldn't put your name in the name of an open source package, Pulni!* 🤨  
\- You're probably right, but this way I can call it "PETs for U", and who doesn't like pets?! 😅  
\- *You're an idiot! ... But also, fair point.* 🐾

## Features
- TypePicker - an attribute for SerializeReference fields of abstract/interface types which allows picking which concrete types to use
  - ManagedReferenceFixerWindow - a window solving a common issue related to `[SerializeReference]` fields
- ExposedProperties - a component that visualizes/groups properties of other components
- DebugHelper - a set of menu items and APIs that allow you to debug specific object instances

## System Requirements
Unity **2021.3** or later.

## Installation
1. You can install this via git url by adding a new package from the package manager and pasting this address
```
https://github.com/pulni4kiya/unity-editor-tools.git
```
2. Alternatively, you can manually add this entry in your **manifest.json**
```
"com.pulni.editor-tools": "https://github.com/pulni4kiya/unity-editor-tools.git"
```
3. The package is also available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.pulni.editor-tools
```

## Note from Pulni
Hey! I hope you'll find these tools useful! I use them pretty much all the time and wanted to share them with others.  
If you do like these, and wanna support me, you can "buy me a cake" on [ko-fi](https://ko-fi.com/pulni)! 😋🍰

# Feature details
## TypePicker
Easily add more flexibility to your components and scriptable objects!

TypePicker is an attribute with a property drawer which - in combination with the `SerializeReference` attribute - allows you to serialize interfaces and abstract classes and choose which concrete implementation you want to use and make it editable in the inspector.

### How to use
All you need to do is put the SerializeReference and TypePicker attributes on a field of an abstract/interface type!
```csharp
public class MyMono : MonoBehaviour {
    [SerializeReference, TypePicker] private IMyInterface myInteraface;
}

// You can have whatever you want in the interface
public interface IMyInterface { } 

public class MyNumber : IMyInterface {
    [SerializeField] private int number;
}

public class MyString : IMyInterface {
    [SerializeField] private string text;
}

public class MyData : IMyInterface {
    [SerializeField] private UnityEngine.Object obj;
    [SerializeField] private Color color;
}
```
And the result is:

![2024-02-25-15-49-09-Unity-0006](https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/6606f523-8496-4477-be4a-740251ad4481)

### Additional options and features
- **Copy and paste** - this package comes with functionality that allows you to copy and paste all `[SerializeReference]` fields.
- **Custom order and names** - the options in the dropdown are sorted by their order (0 by default) and then by their name. You can customize these by adding the `[TypePickerInfo("ClassDisplayName", OrderNumber)]` attribute to the classes that you want.
- **Group types** - using the `TypePickerInfo` attribute, you can also specify the name as `SomeGroup/SomeName` which allow you to build a tree of options.
- **Custom type filter** - you can implement a custom instance function in the class containing a serialized reference that filters the type options that you can pick in whatever way you want.

Here's example code:
```csharp
public class MyMono : MonoBehaviour {
	[SerializeField] private bool allowNodes;
	[SerializeReference, TypePicker(nameof(GetTypeOptions))] private IMyInterface myInteraface;

	private TypePickerOptions GetTypeOptions() {
		if (allowNodes) {
			return TypePickerHelper.GetAvailableTypes(typeof(IMyInterface));
		} else {
			var result = new TypePickerOptions();
			result.subtypes = new System.Type[] { typeof(MyNumber), typeof(MyString) };
			result.FillNamesFromTypes();
			return result;
		}
	}
}

public interface IMyInterface { }

[TypePickerInfo("Primitive/My Number")]
public class MyNumber : IMyInterface {
	[SerializeField] private int number;
}

[TypePickerInfo("Primitive/My String")]
public class MyString : IMyInterface {
	[SerializeField] private string text;
}

[TypePickerInfo(order: 10)] // Make this show after the others
public class MyNode : IMyInterface {
	[SerializeField] private string text;
	[SerializeReference, TypePicker] private IMyInterface childElement1;
	[SerializeReference, TypePicker] private IMyInterface childElement2;
}
```
And here's the result:

![2024-02-25-16-08-08-Unity-0014](https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/3ff7b4f8-589b-4254-9b30-3fb294dfc768)

And an example of copying SerializeReference objects:

![2024-02-25-16-09-58-Unity-0016](https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/ecf7d68e-bf88-4402-a63a-69515b3f419a)


### Problems you'll likely encounter and their solutions
1. Changing the name of a class, namespace or assembly that is serialized using `[SerializeReference]` will cause serialization problems, but these can be solved by using the `[MovedFrom]` attribute (from the `UnityEngine.Scripting.APIUpdating` namespace) and specifying the old name.
2. Every now and then you may get an error like
   >Trying to update the managed reference registry with invalid propertyPath(likely caused by a missing reference instance)'managedReferences[X].Y', with value 'Z'

   which happens because data is serialized (usually as prefab override) but the serialized reference it's supposed to override no longer exists.
   This can be resolved by using the `SerializedReferenceFixerWindow`.
3. SerializeReference has some limitations when it comes to Generics or UnityEngine.Object objects. You can read more details [here](https://docs.unity3d.com/ScriptReference/SerializeReference.html).

### SerializedReferenceFixerWindow
This is a window that basically cleans up old and unused `[SerializeReference]` data (specifically prefab overrides to objects that no longer exist).
Using the window below, you can find all instances of such issues and clear them with the press of a button.

![2024-02-25-16-36-27-Unity-0021](https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/02480df9-8bbe-48fd-b77c-2f2437d472c1)

## ExposedProperties
Add a facade for your complex prefabs in the inspector!

ExposedProperties ia s simple component that just shows properties from other components, allowing you to control the most relevant/common properties of a hierarchy at its root. This makes it much easier to set things up for a new instance of a prefab, and less likely to forget some important field.

Additionally, in the UI of the component, you can:
- Show custom (more descriptive) names for the properties
- Group the proprties into a foldout
- Make some (or all) properties read-only

### How to use

https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/bc04aba2-eec2-481b-915e-c1e179935d2f

## DebugHelper
Improve your debug speed when dealing with many objects 

The DebugHelper allows you to debug specific instances of your objects and components - very useful when you have many instances of the same component getting in the way of debugging the one that has a problem.

### How to use
The DebugHelper adds a "Toggle Debug" menu item that allow you to toggle debugging on any component or game object (which means all of its components).  
As soon as you click that menu item, you'll get a log whether debugging was enabled or disabled for the object / component.  
*Note: The flag is stored by the component instance as key, so it resets to disabled when an instance is destroyed.*

Once you've enabled debugging for a component, you can put conditional breakpoints inside its code with the `DBG.Check(this)` as condition.
*Note: Conditional breakpoints are rather slow, you may want to use the methods mentioned below instead.*

Additionally, you can use `DBG.LogIfDebugged`,  `DBG.BreakIfDebugged` and `DBG.ActionIfDebugged` to log messages, trigger a debugger break or do some custom action only if a component is being debugged. These calls will get removed from builds (using the Conditional attribute).

Here's some code that we'll try to debug:
```csharp
public class TestDebugging : MonoBehaviour {
	public void Update() {
		DBG.LogIfDebugged(this, "Object being debugged: " + this.name, this);
		if (Time.frameCount == 100) {
			// Lets pretend we want to check this code
			this.name.ToString();
		}
	}
}
```
And here's a video of the process:

https://github.com/pulni4kiya/unity-editor-tools/assets/31959408/59b44ee4-82f3-41fe-a7c6-8c7d5ecc2146

*Note: If the class DBG in the global namespace is not convenient for you, you can add the `PULNI_NO_DBG` compilation symbol, and the class will be removed. Instead you'll be able to use the class `DebugHelper` (in Pulni.EditorTools namespace) which provides the actual functionality.*
