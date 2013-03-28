Imports System.IO

Public Class PositionFileKreator
    Public Function CreatePositionFiles(ByVal x As Boolean, ByVal datedictionary As Dictionary(Of String, List(Of String)), ByVal saveFileLocation As String)
        'This function will take unzipped position files and organize them into 4-hour or 24-hour segments
        'for easy sorting. For contigual track analyses, I highly recommend using 4-hour segments as the 
        'stored procedure involved in generating the report requires exponentially more time the larger the file is.

        'This function also strips out the double quotes surrounding the date/time field in the original
        'position data. These quotes MUST be taken out for the file to be successfully loaded into SQL Server.

        'This function has been stripped out as a stand-alone program called Position File Fixer. That program is more up
        'to date and reliable than this embedded version.
        Dim savefilename As String
        Dim savefilename2 As String
        Dim savefilename3 As String
        Dim savefilename4 As String
        Dim savefilename5 As String
        Dim savefilename6 As String
        Dim filemaker As StreamWriter
        Dim filemaker2 As StreamWriter
        Dim filemaker3 As StreamWriter
        Dim filemaker4 As StreamWriter
        Dim filemaker5 As StreamWriter
        Dim filemaker6 As StreamWriter
        Dim datenoquotes As String
        Dim newlinenewdate As String
        Dim line As String
        Dim filereader As StreamReader
        Dim filelist As New List(Of String)
        Dim hour As Integer
        If x = True Then
            For Each strKey In datedictionary.Keys()
                savefilename = saveFileLocation + "\" + strKey + "FullDay.csv"
                filemaker = New StreamWriter(savefilename, False)
                Dim keylist = datedictionary(strKey)
                For Each strfilename In keylist
                    filereader = My.Computer.FileSystem.OpenTextFileReader(strfilename)
                    Do Until filereader.EndOfStream()
                        line = filereader.ReadLine()
                        If line <> "" Then
                            If line.Length > 20 Then
                                Dim linesplit = line.Split(New [Char]() {","})
                                If linesplit(2).Length > 19 Then
                                    datenoquotes = linesplit(2).ToString.Substring(1, 19)
                                Else
                                    datenoquotes = linesplit(2)
                                End If
                                If strKey = datenoquotes.Substring(0, 10) Then
                                    newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                    filemaker.WriteLine(newlinenewdate)
                                End If
                            End If
                        End If
                    Loop
                    filereader.Close()
                Next
                filemaker.Close()
                filereader.Close()
                filelist.Add(savefilename)
            Next
            filereader.Close()
            filemaker.Close()
        ElseIf x = False Then
            For Each strKey In datedictionary.Keys()
                savefilename = saveFileLocation + strKey + " 0AM to 4AM.csv"
                savefilename2 = saveFileLocation + strKey + " 4AM to 8AM.csv"
                savefilename3 = saveFileLocation + strKey + " 8AM to 12PM.csv"
                savefilename4 = saveFileLocation + strKey + " 12PM to 16PM.csv"
                savefilename5 = saveFileLocation + strKey + " 16PM to 20PM.csv"
                savefilename6 = saveFileLocation + strKey + " 20PM to 24PM.csv"
                filemaker = New StreamWriter(savefilename, False)
                filemaker2 = New StreamWriter(savefilename2, False)
                filemaker3 = New StreamWriter(savefilename3, False)
                filemaker4 = New StreamWriter(savefilename4, False)
                filemaker5 = New StreamWriter(savefilename5, False)
                filemaker6 = New StreamWriter(savefilename6, False)
                Dim keylist = datedictionary(strKey)
                For Each strfilename In keylist
                    filereader = My.Computer.FileSystem.OpenTextFileReader(strfilename)
                    Do Until filereader.EndOfStream()
                        line = filereader.ReadLine()
                        If line <> "" Then
                            If line.Length > 20 Then
                                Dim linesplit = line.Split(New [Char]() {","})
                                If linesplit(2).Length > 19 Then
                                    datenoquotes = linesplit(2).ToString.Substring(1, 19)
                                Else
                                    datenoquotes = linesplit(2)
                                End If
                                hour = CInt(datenoquotes.Substring(11, 2))
                                If strKey = datenoquotes.Substring(0, 10) Then
                                    If hour >= 0 AndAlso hour < 4 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker.WriteLine(newlinenewdate)
                                    ElseIf hour >= 4 AndAlso hour < 8 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker2.WriteLine(newlinenewdate)
                                    ElseIf hour >= 8 AndAlso hour < 12 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker3.WriteLine(newlinenewdate)
                                    ElseIf hour >= 12 AndAlso hour < 16 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker4.WriteLine(newlinenewdate)
                                    ElseIf hour >= 16 AndAlso hour < 20 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker5.WriteLine(newlinenewdate)
                                    ElseIf hour >= 20 AndAlso hour < 24 Then
                                        newlinenewdate = linesplit(0) + "," + linesplit(1) + "," + datenoquotes + "," + linesplit(3) + "," + linesplit(4) + "," + linesplit(5) + "," + linesplit(6) + "," + linesplit(7)
                                        filemaker6.WriteLine(newlinenewdate)
                                    End If
                                End If
                            End If
                        End If
                    Loop
                    filereader.Close()
                Next
                filemaker.Close()
                filemaker2.Close()
                filemaker3.Close()
                filemaker4.Close()
                filemaker5.Close()
                filemaker6.Close()
                filereader.Close()
                filelist.Add(savefilename)
                filelist.Add(savefilename2)
                filelist.Add(savefilename3)
                filelist.Add(savefilename4)
                filelist.Add(savefilename5)
                filelist.Add(savefilename6)
                For Each g In filelist
                    Dim h As New FileInfo(g)
                    Dim glength As Long = h.Length
                    If glength < 1 Then
                        My.Computer.FileSystem.DeleteFile(g)
                    End If
                Next
            Next
            filereader.Close()
            filemaker.Close()
            filemaker2.Close()
            filemaker3.Close()
            filemaker4.Close()
            filemaker5.Close()
            filemaker6.Close()
        End If
        Return True
    End Function
End Class
