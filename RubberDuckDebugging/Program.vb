Imports Lucene.Net.Analysis
Imports Lucene.Net.Analysis.Ja
Imports Lucene.Net.Analysis.Ja.Dict
Imports Lucene.Net.Analysis.Ja.TokenAttributes
Imports Lucene.Net.Analysis.TokenAttributes
Imports Lucene.Net.Util
Imports Microsoft.Extensions.Configuration

Module Program

    Private Property Accumulator As New List(Of AttributeSource)

    ''' <summary>
    ''' ���C���֐�
    ''' </summary>
    ''' <param name="args"></param>
    Sub Main(args As String())

        '�e���v���[�g�̓ǂݍ���
        Dim Configuration As IConfiguration = New ConfigurationBuilder().
            SetBasePath(IO.Directory.GetCurrentDirectory()).
            AddJsonFile("appsettings.json", True, True).
            Build()

        '�e�Z�N�V�����̐ݒ�
        Dim emptySection = Configuration.GetSection("empty").Get(Of String())
        Dim initialSection = Configuration.GetSection("initial").Get(Of String())
        Dim intermediateSection = Configuration.GetSection("intermediate").Get(Of String())
        Dim advancedSection = Configuration.GetSection("advanced").Get(Of String())
        Dim quitsSection = Configuration.GetSection("quits")

        '�����_���ȏ������쐬
        Dim Randomize = Function(count As Integer)
                            Return New Random().Next(0, count)
                        End Function

        '�ŏ��̎���
        Console.WriteLine("�@�@�Q        ")
        Console.WriteLine("�@ <�E)       ")
        Console.WriteLine("�@ (( 3)     ")
        Console.WriteLine("~~~~~~~~~~~~")
        Console.WriteLine("���������Ă��邱�Ƃ͂���܂����H")

        '�R�}���h�ǂݎ��
        While (True)
            Console.Write(">")
            Dim readline As String = Console.ReadLine().ToLower.Trim
            Select Case readline
                Case "exit", "quit", "bye", "thanks"
                    Console.WriteLine(quitsSection.Item(readline))
                    Threading.Thread.Sleep(1000)
                    Return
                Case String.Empty
                    Console.WriteLine(emptySection(Randomize(emptySection.Count)))
                    Continue While
            End Select

            '�`�ԑf���
            Dim tokenizer As Tokenizer = New JapaneseTokenizer(New IO.StringReader(readline), ReadDict(), False, JapaneseTokenizerMode.NORMAL)
            Dim TokenStreamComponents = New TokenStreamComponents(tokenizer, tokenizer)
            Using tokenStream = TokenStreamComponents.TokenStream
                tokenStream.Reset()
                While (tokenStream.IncrementToken())
                    Accumulator.Add(tokenStream.CloneAttributes)
                End While
                tokenStream.End()
            End Using

            'Dim itemNNs = Accumulator.Select(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech).ToArray
            'Console.WriteLine(String.Join(",", itemNNs))

            '���̎���
            If Not Accumulator.Any(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("����") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("���")) Then
                Console.WriteLine(initialSection(Randomize(initialSection.Count)))
            Else
                Dim question As String
                If Accumulator.Count <= 50 Then
                    question = intermediateSection(Randomize(intermediateSection.Count))
                Else
                    question = advancedSection(Randomize(advancedSection.Count))
                End If

                Dim itemNN = Accumulator.Where(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("����") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("���")).
                    Select(Function(x) x.GetAttribute(Of ICharTermAttribute).ToString).
                    GroupBy(Function(s) s). ' groups identical strings into an IGrouping
                    OrderByDescending(Function(group) group.Count()). ' IGrouping Is a collection, so you can count it
                    Select(Function(group) group.Key).
                    FirstOrDefault
                Dim itemVB = Accumulator.Where(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("����") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("����")).
                    Select(Function(x) x.GetAttribute(Of ICharTermAttribute).ToString).
                    GroupBy(Function(s) s). ' groups identical strings into an IGrouping
                    OrderByDescending(Function(group) group.Count()). ' IGrouping Is a collection, so you can count it
                    Select(Function(group) group.Key).
                    FirstOrDefault
                Console.WriteLine(question.Replace("{{NN}}", itemNN).Replace("{{VB}}", itemVB))
            End If
        End While

    End Sub


    Public Function ReadDict() As UserDictionary
        Dim reader As IO.TextReader = New IO.StreamReader("userdict.txt", System.Text.Encoding.UTF8)
        Return New UserDictionary(reader)
    End Function

End Module
