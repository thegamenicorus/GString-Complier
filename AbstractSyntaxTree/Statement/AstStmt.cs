
/* <stmt> := var <ident> = <expr>
	| <ident> = <expr>
	| for <ident> = <expr> to <expr> do <stmt> end
	| read_int <ident>
	| print <expr>
	| <stmt> ; <stmt>
  */
using System;
public enum CollectionType { Array, List,None }
public abstract class Stmt
{
}

// <stmt> ; <stmt>
public class Sequence : Stmt
{
	public Stmt First { get; set; }
	public Stmt Second { get; set; }
}
