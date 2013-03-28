Imports System.IO

Public Class DiskoverDates
    Public Function DiscoverDates(ByVal x As String, filenamelist As List(Of String))
        'This function loops through all the position files and creats a Dictionary that 
        'keeps track of dates each file contains.

        'This information is used by the PositionFileFixer to correctly build new position files that
        'are organzied by date and time.
        Dim line As String
        Dim initialdate As String
        Dim filereader As StreamReader
        Dim datedictionary As New Dictionary(Of String, List(Of String))
        For Each g In filenamelist
            filereader = My.Computer.FileSystem.OpenTextFileReader(g)
            Dim filename = Path.GetFileName(g)
            Do Until filereader.EndOfStream()
                line = filereader.ReadLine()
                Dim linesplit = line.Split(New [Char]() {","})
                If line <> "" Then
                    If line.Length > 20 Then
                        If linesplit(2).Length > 19 Then
                            initialdate = linesplit(2).Substring(1, 10)
                        Else
                            initialdate = linesplit(2).Substring(0, 10)
                        End If
                        If Not datedictionary.ContainsKey(initialdate) Then
                            datedictionary.Add(initialdate, New List(Of String)(New String() {g}))
                        End If
                        Dim dictkeys = datedictionary(initialdate)
                        If Not dictkeys.Contains(g) Then
                            dictkeys.Add(g)
                        End If
                    End If
                End If
            Loop
        Next
        Return datedictionary
    End Function
End Class
