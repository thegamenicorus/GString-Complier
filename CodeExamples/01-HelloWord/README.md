## Hello world!

### helloword.gstr

```
Class: {Application}.
	Main.
		Display: "Hello, world!!".
	End method.
End class.
```

#### Result:
```
Hello, world!!
```

### display.gstr
```
Class: {Application}.
	Main.
	        ** Display.
		Display: "Message1".
	        Display: "Message2".
	        Display: "Message3".

	        ** Display with new line.
	        Display with line: "Message4".
	        Display with line: "Message5".
	        Display with line: "Message6".
	End method.
End class.
```

#### Result:
```
Message1Message2Message3Message4
Message5
Message6
```
