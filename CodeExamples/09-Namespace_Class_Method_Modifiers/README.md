## Modifiers!

### 📝 modifiers.gstr

This example demonstrates how to specify a Namespace, class modifiers, and method modifiers. The available modifiers include Public, Private, Protected, and Static.

```
Namespace: {GString}.
    Class: {TestClassWithModifier}, modifiers:{Public}.

        Method: {MethodWithModifier}, modifiers:{Public,Static}, return type:{ void }.
                Display: "I am MethodWithModifier".
        End method.

    End class.
End namespace.
```
