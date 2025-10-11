using System;

[AttributeUsage(AttributeTargets.Field)]
public class ActionInputAttribute : Attribute {
	public Type Input;
	public ActionInputAttribute(Type input) {
		Input = input;
	}
}

