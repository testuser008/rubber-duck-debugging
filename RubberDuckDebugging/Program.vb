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
    ''' メイン関数
    ''' </summary>
    ''' <param name="args"></param>
    Sub Main(args As String())

        'テンプレートの読み込み
        Dim Configuration As IConfiguration = New ConfigurationBuilder().
            SetBasePath(IO.Directory.GetCurrentDirectory()).
            AddJsonFile("appsettings.json", True, True).
            Build()

        '各セクションの設定
        Dim emptySection = Configuration.GetSection("empty").Get(Of String())
        Dim initialSection = Configuration.GetSection("initial").Get(Of String())
        Dim intermediateSection = Configuration.GetSection("intermediate").Get(Of String())
        Dim advancedSection = Configuration.GetSection("advanced").Get(Of String())
        Dim quitsSection = Configuration.GetSection("quits")

        'ランダムな序数を作成
        Dim Randomize = Function(count As Integer)
                            Return New Random().Next(0, count)
                        End Function

        '最初の質問
        Console.WriteLine("　　＿        ")
        Console.WriteLine("　 <・)       ")
        Console.WriteLine("　 (( 3)     ")
        Console.WriteLine("~~~~~~~~~~~~")
        Console.WriteLine("何か困っていることはありますか？")

        'コマンド読み取り
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

            '形態素解析
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

            '次の質問
            If Not Accumulator.Any(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("名詞") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("一般")) Then
                Console.WriteLine(initialSection(Randomize(initialSection.Count)))
            Else
                Dim question As String
                If Accumulator.Count <= 50 Then
                    question = intermediateSection(Randomize(intermediateSection.Count))
                Else
                    question = advancedSection(Randomize(advancedSection.Count))
                End If

                Dim itemNN = Accumulator.Where(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("名詞") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("一般")).
                    Select(Function(x) x.GetAttribute(Of ICharTermAttribute).ToString).
                    GroupBy(Function(s) s). ' groups identical strings into an IGrouping
                    OrderByDescending(Function(group) group.Count()). ' IGrouping Is a collection, so you can count it
                    Select(Function(group) group.Key).
                    FirstOrDefault
                Dim itemVB = Accumulator.Where(Function(x) x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.StartsWith("動詞") AndAlso
                                                   x.GetAttribute(Of IPartOfSpeechAttribute).GetPartOfSpeech.Contains("自立")).
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
