using Collections = System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace GStringCompiler
{
    // Display <expr>
    public class Display : Stmt, Expr
    {
        public Expr Expr { get; set; }
        public bool WithLine { get; set; }
        public DisplayObjectArray DisplayObjArr = new DisplayObjectArray();

        public static Stmt Synthesize(ref int index, Collections.IList<GStringObject> tokens)
        {

            Display display = new Display();

            index++;
            if (tokens[index].Object.Equals("with"))
            {
                index++;
                if (tokens[index].Object.Equals("line"))
                {
                    index++;
                    display.WithLine = true;
                }
                else
                {
                    throw new Error("wrong syntax [after 'with'] [At line: " + tokens[index].Line + "]");
                }
            }
            if (tokens[index].Object != Scanner.Colon)
                if (display.WithLine)
                    throw new Error("require :[Colon] after 'line' [At line: " + tokens[index].Line + "]");
                else
                    throw new Error("require :[Colon] after 'Display' [At line: " + tokens[index].Line + "]");

            index++;

            display.DisplayObjArr.Element = display.InstanceElement(ref index, tokens, new List<object>());

            return display;
        }

        public static Display SynthesizeSpecialRead(ref int index, ref Collections.IList<GStringObject> tokens)
        {

            Display display = new Display() { WithLine = false };

            display.DisplayObjArr.Element = display.InstanceElement(ref index, tokens, new List<object>());
            index--;
            return display;
        }

        private List<object> InstanceElement(ref int index, IList<GStringObject> tokens, List<object> element)
        {

            if (tokens[index].Object == Scanner.FullStop)
            {
                return element;
            }
            if (tokens[index].Object == Scanner.Right_Braces || tokens[index].Object == Scanner.Right_Bracket)
            {
                index++;
                return element;
            }
            if (tokens[index].Object == Scanner.Left_Braces)
            {
                index++;

                while (tokens[index].Object != Scanner.Right_Braces)
                {
                    var res = InstanceElement(ref index, tokens, new List<object>());
                    if (res.Count > 0)
                        element = element.Concat(res).ToList();
                    else
                        return element;
                    if (tokens[index].Object == Scanner.Left_Braces)
                    {
                        return InstanceElement(ref index, tokens, element);
                    }
                }
                return element;
            }
            else
            {

                if (tokens[index].Object is StringBuilder)
                    element.Add(Parser.Getinstance().ParseExpr());
                else
                    element.Add(OrderController.GetSequenceOfOperation(ref index, tokens, null));
                return InstanceElement(ref index, tokens, element);
            }
        }
    }
}
