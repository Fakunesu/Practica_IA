public class QuestionNode : ITreeeNode
{
    private System.Func<bool> question;
    private ITreeeNode trueNode;
    private ITreeeNode falseNode;

    public QuestionNode(System.Func<bool> question, ITreeeNode trueNode, ITreeeNode falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public void Execute()
    {
        if (question())
        {
            trueNode.Execute();
        }
        else
        {
            falseNode.Execute();
        }
    }
}