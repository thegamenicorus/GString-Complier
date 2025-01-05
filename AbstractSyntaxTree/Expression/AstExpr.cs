
/* <expr> := <string>
 *  | <int>
 *  | <arith_expr>
 *  | <ident>
 */
using GStringCompiler;

public interface Expr
{
    
}

public class VarDecExpr:Expr{
}

// <string> := " <string_elem>* "
public class StringLiteral : VarDecExpr
{
    public string Value { get; set; }
}

// <int> := <digit>+
public class IntLiteral : VarDecExpr
{
    public int Value { get; set; }
}

public class FloatLiteral : VarDecExpr
{
    public float Value { get; set; }
}

public class DoubleLiteral : VarDecExpr
{
    public double Value { get; set; }
}

public class BooleanLiteral : VarDecExpr
{
    public bool Value { get; set; }
}

public class SequenceExpr : VarDecExpr
{
    public TreeNode Value { get; set; }
}

public class NotSymbolSequenceExpr : SequenceExpr
{ }

public class NegSymbolSequenceExpr : SequenceExpr
{ }

public class OptStrSequenceExpr : VarDecExpr
{
    public TreeNode Value { get; set; }
}

//// <bin_expr> := <expr> <bin_op> <expr>
//public class BinExpr : Expr
//{
//    public Expr Left;
//    public Expr Right;
//    public BinOp Op;
//}


public class LogicalSymbolExpr : Expr
{

}
public class LogicalConditionSymbolExpr : LogicalSymbolExpr { }
public class AddSymbol : LogicalSymbolExpr { }
public class SubSymbol : LogicalSymbolExpr { }
public class SubNegSymbol : LogicalSymbolExpr { }
public class MulSymbol : LogicalSymbolExpr { }
public class DivSymbol : LogicalSymbolExpr { }
public class ModSymbol : LogicalSymbolExpr { }
public class EmptySymbol : LogicalSymbolExpr { }
public class LessThanAndEqualSymbol : LogicalConditionSymbolExpr { }
public class MoreThanAndEqualSymbol : LogicalConditionSymbolExpr { }
public class LessThanSymbol : LogicalConditionSymbolExpr { }
public class MoreThanSymbol : LogicalConditionSymbolExpr { }
public class AndSymbol : LogicalConditionSymbolExpr { }
public class OrSymbol : LogicalConditionSymbolExpr { }
public class AndAlsoSymbol : LogicalConditionSymbolExpr { }
public class OrElseSymbol : LogicalConditionSymbolExpr { }
public class LogicalEqualSymbol : LogicalConditionSymbolExpr { }
public class LogicalNotSymbol : LogicalConditionSymbolExpr { }
public class LogicalNotEqualSymbol : LogicalConditionSymbolExpr { }

public class LogicalSymbol : Expr
{
    public string Symbol { get; private set; }
    public string Name { get; private set; }
    public LogicalSymbol(string name,string symbol)
    {
        Name = name;
        Symbol = symbol;
    }
}

public class TreeNode : Expr
{
    public LogicalSymbolExpr Op;
    public TreeNode Left;
    public TreeNode Right;
    public Expr Value;
    public System.Collections.Generic.List<Expr> ShuntingYardArtmValue { get; set; }
    public System.Reflection.Emit.Label JumpOut { get; set; }
    public System.Reflection.Emit.Label JumpNextMainCondition { get; set; }
    public System.Reflection.Emit.Label JumpNextPairCondition { get; set; }
    public System.Reflection.Emit.Label JumpNextParentCondition { get; set; }
    public System.Reflection.Emit.Label JumpToCode { get; set; }
    public bool IsDefineMarkJumpOut { get; set; }
    public TreeNode(LogicalSymbolExpr op, TreeNode left, TreeNode right)
    {
        Op = op;
        Left = left;
        Right = right;
        IsDefineMarkJumpOut = true;
    }

    public TreeNode(Expr value)
    {
        Value = value;
    }
}

// <bin_op> := + | - | * | /
//public enum BinOp
//{
//    Add,
//    Sub,
//    Mul,
//    Div
//}