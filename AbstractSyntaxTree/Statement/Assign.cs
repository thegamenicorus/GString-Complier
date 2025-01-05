using Collections = System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    // <ident> = <expr>
    public class Assign : Stmt
    {
        public string Ident { get; set; }
        public Expr Expr { get; set; }
        public System.Collections.Generic.List<object> Index { get; set; }
        public System.Type TypeOfReferencedVar { get; private set; }
        public static Stmt Synthesize(ref int index, ref Collections.IList<GStringObject> tokens)
        {
            index++;
            if (!tokens[index].Object.Equals("to") || index == tokens.Count)
                throw new Error("require 'to' after 'Assign' [At line: " + tokens[index].Line + "]");

            index++;
            // Assignment.
            Assign assign = new Assign();
            List<Stmt> stmtList = new List<Stmt>();
            if (index < tokens.Count && tokens[index].Object is string)
            {
                string ident = (string)tokens[index++].Object;
                if (ident.IsReserved())
                    throw new Error("'" + ident + "' is reserved keyword [At line: " + tokens[index].Line + "]");
                assign.Ident = ident;
            }
            else
                throw new Error("require variable name after 'Assign to' [At line: " + tokens[index].Line + "]");
            stmtList.Add(assign);
            assign.TypeOfReferencedVar = TypeIdentifier.FindTypeOfVariable(assign.Ident, tokens[index].Line);
            if (tokens[index].Object != Scanner.Comma || index == tokens.Count)
            {
                throw new Error("require ,[Comma] after variable: " + assign.Ident + " [At line: " + tokens[index].Line + "]");
            }

            // Assign position.
            string keyword = "position";
            assign.Index = UnsortedFinder.Find(new List<FinderObject>{
                new FinderObject{Keyword=keyword, ExpectedType=typeof(int)}}, ref index, tokens,
                new List<string> { "value" }
                )[0] as List<object>;

            if (!TypeIdentifier.IsGStringCollection(assign.Ident, tokens[index].Line) && assign.Index != null)
                throw new Error("keyword: 'position' is not require for variable: '" + assign.Ident + "' [At line: " + tokens[index].Line + "]");

            if (assign.Index != null) index++;

            if (tokens[index].Object != Scanner.Comma || index == tokens.Count)
                throw new Error("require ,[Comma] after variable: " + assign.Ident + " [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("value") || index == tokens.Count)
                throw new Error("require statement after ,[Comma] [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                throw new Error("require :[Colon] after 'value' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces || index == tokens.Count)
                throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object.Equals("Read"))
            {
                index--;
                stmtList.Remove(assign);
                stmtList.Add(BracketReadOperation.Synthesize(ref index, tokens, assign));
            }
            else
            {
                if (OrderController.IsSeqExpr(index, tokens))
                {
                    assign.Expr = OrderController.GetSequenceOfOperation(ref index, tokens, TypeIdentifier.FindTypeOfVariable(assign.Ident, tokens[index].Line));
                }
                else
                {
                    assign.Expr = Parser.Getinstance().ParseExpr();
                    var typeForRef = assign.TypeOfReferencedVar == typeof(System.Text.StringBuilder) ? typeof(string) : assign.TypeOfReferencedVar;
                    var typeOfExpr = TypeIdentifier.TypeOfExpr(assign.Expr);

                    // If assign value is store in pass by ref, check for & and replace it.
                    if (typeForRef.FullName.Last() == '&')
                        typeForRef = System.Type.GetType(typeForRef.FullName.Remove(typeForRef.FullName.Length - 1));

                    if (typeOfExpr.FullName.Last() == '&')
                        typeOfExpr = System.Type.GetType(typeOfExpr.FullName.Remove(typeOfExpr.FullName.Length - 1));

                    if (typeForRef != typeOfExpr)
                        throw new Error("require type:'" + assign.TypeOfReferencedVar.ToString() + "' for value of variable: '" + assign.Ident + "'  [At line: " + tokens[index].Line + "]");
                }
            }

            if (stmtList[0] is Assign)
                index++;

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }
}
