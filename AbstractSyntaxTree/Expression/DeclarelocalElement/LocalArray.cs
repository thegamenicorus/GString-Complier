using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class LocalArray : GStringCollection
    {

        public LocalArray() { }

        public void InstanceElement(ref int index, IList<GStringObject> tokens)
        {
            Element = InstanceElement(ref index, tokens, new List<object>());
        }

        private List<object> InstanceElement(ref int index, IList<GStringObject> tokens, List<object> element)
        {

            if (tokens[index].Object == Scanner.Right_SquareBracket)
            {
                index++;
                return element;
            }
            if (tokens[index].Object == Scanner.Left_SquareBracket)
            {
                index++;

                while (tokens[index].Object != Scanner.Right_Braces)
                {
                    var res = InstanceElement(ref index, tokens, new List<object>());
                    if (res.Count > 0)
                        element.Add(res);
                    else
                        return element;
                    if (tokens[index].Object == Scanner.Left_SquareBracket)
                    {
                        return InstanceElement(ref index, tokens, element);
                    }
                }
                return element;
            }
            else
            {
                if (tokens[index].Object == Scanner.Comma) { index++; }
                else if (OrderController.IsSeqExpr(index - 1, tokens, new object[] { Scanner.Comma, Scanner.Right_SquareBracket }))
                {
                    element.Add(OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Comma, Scanner.Right_SquareBracket }, null));
                }
                else if (Type == tokens[index].Object.GetType() || tokens[index].Object is StringBuilder || tokens[index].Object is string)
                    element.Add(Parser.Getinstance().ParseExpr(ref index));
                else if (tokens[index].Object == Scanner.Right_Braces) { return element; }
                else
                    throw new Error("require type:'" + Type.ToString() + "' for value of array: '" + Ident + "'  [At line: " + tokens[index].Line + "]");
                return InstanceElement(ref index, tokens, element);
            }
        }
    }
}
