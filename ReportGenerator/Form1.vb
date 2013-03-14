Imports System
Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.OleDb
Imports System.IO.StreamWriter
Imports System.IO.Path
Public Class Form1
    Dim fd As OpenFileDialog = New OpenFileDialog()
    Dim fd2 As OpenFileDialog = New OpenFileDialog()
    Dim sd As FolderBrowserDialog = New FolderBrowserDialog()
    Dim ds As New DataSet
    Dim posFilename As String = ""
    Dim rawFilename As String = ""
    Dim saveFileLocation As String = ""
    Dim reportduration24 As Boolean = False
    Dim filereader As StreamReader
    Dim reportname As String
    Dim poscount As Integer = 0
    Dim rawcount As Integer = 0
    Dim tablecount As Integer = 0
    Dim filelist As New List(Of String)
    Dim hour As Integer
    Dim filedictionary As New Dictionary(Of String, List(Of String))
    Dim filedictionary2 As New Dictionary(Of String, List(Of String))
    Dim CTpercent As Decimal
    Dim totalrepeats As Integer = 0
    Dim totalnonrepeats As Integer = 0
    Dim totalCT As Integer = 0
    Dim fulldayrepeats As Integer = 0
    Dim fulldaynonrepeats As Integer = 0
    Dim fulldayct As Integer = 0
    Dim numberofassets As Integer = 0
    Dim foundrow As DataRow
    Dim fullreportname As String
    Dim processpositionfiles As Boolean = True
    Dim ctdictionary As New Dictionary(Of String, Decimal)
    Dim tagerrordictionary As New Dictionary(Of String, Decimal)
    Dim interval As String
    Dim combinereports As Boolean = False
    Dim rawfilelist As New List(Of String)
    Dim datedictionary As New Dictionary(Of String, List(Of String))
    Dim runningposfiles As Integer = 0
    '*****************************************************************************************************
    'BIG TO DO: THREAD ENTIRE APPLICATION SO IT REMAINS INTERACTIVE WHILE REPORTS ARE BEING PROCESSED!!!!!
    '*****************************************************************************************************
    
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'This subroutine clears the file list views, then presents an OpenFileDialog from which to choose files for
        'processing. Files are restricted to the type required by the reports selected for processing.

        'If OpenFileDialog is closed without selecting files, an error is displayed. 

        'Multiple files can be selected at once.
        'Selected files are placed in the ListView.

        'TO DO: Remove files from ListView when they finish processing. Make the ListView scrollable while the 
        '       reports are generating

        ListView1.Clear()
        ListView2.Clear()
        ListView1.Columns.Add("File to Process", 295)
        ListView2.Columns.Add("Save File Location", 295)
        If CheckBox1.Checked = False AndAlso CheckBox2.Checked = False AndAlso CheckBox3.Checked = False _
            AndAlso CheckBox4.Checked = False AndAlso CheckBox5.Checked = False AndAlso CheckBox6.Checked = False _
            AndAlso CheckBox7.Checked = False AndAlso CheckBox8.Checked = False Then
            MsgBox("You must choose a report to run before selecting a file", , "ERROR")
        ElseIf CheckBox1.Checked = True Or CheckBox2.Checked = True Or CheckBox3.Checked = True _
                        Or CheckBox4.Checked = True Or CheckBox7.Checked = True Then
            fd.Title = "Choose Position files to Process"
            fd.InitialDirectory = "I:\"
            fd.Filter = "Position files (*.csv)|*.csv|Position Files (*.csv)|*.csv"
            fd.RestoreDirectory = True
            fd.Multiselect = True
            If fd.ShowDialog() = Windows.Forms.DialogResult.OK Then
                posFilename = fd.FileName
                For Each posfilename As String In fd.FileNames
                    ListView1.Items.Add(Path.GetFileName(posfilename))
                Next
            End If
            If posFilename = "" Then
                MsgBox("No files selected", , "ERROR")
            End If
        End If
        If CheckBox6.Checked = True Or CheckBox5.Checked = True Or CheckBox8.Checked = True Then
            fd2.Title = "Choose Raw files to Process"
            fd2.InitialDirectory = "I:\"
            fd2.Filter = "Raw files (*.txt)|*.txt|Raw Files (*.txt)|*.txt"
            fd2.RestoreDirectory = True
            fd2.Multiselect = True
            If fd2.ShowDialog() = Windows.Forms.DialogResult.OK Then
                rawFilename = fd2.FileName
                For Each rawfilename As String In fd2.FileNames
                    ListView1.Items.Add(Path.GetFileName(rawfilename))
                Next
            End If
            If rawFilename = "" Then
                MsgBox("No files selected", , "ERROR")
            End If
            rawcount = fd2.FileNames.Count
        End If
    End Sub

    Private Function buildprogressbar(ByVal h As Boolean)
        'The progress bar function needs to be reworked.

        'Set max to the number of files in queue to process + the SQL table creations needed for each file & report combo
        '+ the actual reports that are run. 

        If CheckBox1.Checked = True Then
            poscount += 3
        End If
        If CheckBox2.Checked = True Then
            poscount += 2
        End If
        If CheckBox3.Checked = True Then
            poscount += 2
        End If
        If CheckBox4.Checked = True Then
            poscount += 3
        End If
        If CheckBox5.Checked = True Then
            rawcount += 2
        End If
        If CheckBox6.Checked = True Then
            poscount += 2
        End If
        If CheckBox7.Checked = True Then
            rawcount += 3
        End If
        For Each f In filelist
            runningposfiles += 1
        Next
        ProgressBar1.Minimum = 0

        If filelist.Count > 1 Then
            ProgressBar1.Maximum = 1 + (runningposfiles * poscount) + rawcount
        ElseIf filelist.Count < 2 Then
            ProgressBar1.Maximum = runningposfiles * poscount + rawcount
        ElseIf filelist.Count > 1 AndAlso processpositionfiles = True Then
            ProgressBar1.Maximum = 1 + (runningposfiles * poscount) + 1 + rawcount
        ElseIf filelist.Count < 2 AndAlso processpositionfiles = True Then
            ProgressBar1.Maximum = runningposfiles * poscount + 1 + rawcount
        End If

        Return True

    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'This subroutine displays a FolderBrowserDialog that allows the user to select where the .csv report files
        'should be saved.
        'If the user indiciated they want to log new positions using the Logging Service, this function will update
        'the config file associated with the logging service with the correct save location.
        saveFileLocation = ""
        With sd
            .Description = "Choose Save Location"
            .RootFolder = Environment.SpecialFolder.Desktop
            .ShowNewFolderButton = True
        End With
        If sd.ShowDialog() = Windows.Forms.DialogResult.OK Then
            saveFileLocation = sd.SelectedPath + "\"
            If CheckBox8.Checked = True Then
                Dim appsettings() As String = File.ReadAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config")
                appsettings(15) = "  <add key=" + """PositionOutputDirectory""" + " value=" + """" + sd.SelectedPath + """" + "/>"
                File.WriteAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config", appsettings)
            End If
        End If
        ListView2.Items.Add(saveFileLocation)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        'This fuction clears out all input entered into the application.
        ListView1.Clear()
        ListView2.Clear()
        saveFileLocation = ""
        posFilename = ""
        rawFilename = ""

        If CheckBox1.Checked = True Then
            CheckBox1.Checked = False
        End If
        If CheckBox2.Checked = True Then
            CheckBox2.Checked = False
        End If
        If CheckBox3.Checked = True Then
            CheckBox3.Checked = False
        End If
        If CheckBox4.Checked = True Then
            CheckBox4.Checked = False
        End If
        If CheckBox5.Checked = True Then
            CheckBox5.Checked = False
        End If
        If CheckBox6.Checked = True Then
            CheckBox6.Checked = False
        End If
        If CheckBox7.Checked = True Then
            CheckBox7.Checked = False
        End If
        If CheckBox8.Checked = True Then
            CheckBox8.Checked = False
        End If
        If RadioButton1.Checked = True Then
            RadioButton1.Checked = False
        End If
        If RadioButton2.Checked = True Then
            RadioButton2.Checked = False
        End If
        If RadioButton3.Checked = True Then
            RadioButton3.Checked = False
        End If
        If RadioButton4.Checked = True Then
            RadioButton4.Checked = False
        End If

    End Sub

    Private Function WriteCSV(ByVal filename As String, ByVal reportname As String) As String
        'This function writes out the data contained in the DataGridView to a .csv file that is named
        'according to the report run. The function writes out a header, then loops through the data
        'creating a standard .csv file that can be used in Excel for analyzing.

        'For RepeatNonRepeat (the Contiguous Track report) and RepeatsPerXMinute reports, this function
        'keeps track of the files it creates and passes that information to another function that combines them into one full day report. 
       

        Dim str As String = saveFileLocation + Path.GetFileName(filename) & reportname + ".csv"
        Dim streamwriter As New System.IO.StreamWriter(str)
        Dim header As String = DataGridView1.Columns(0).HeaderText
        Dim filepiece As String = Path.GetFileName(filename)
        Dim CTdate As String = filepiece.Substring(0, 10)
        Dim headercount As Integer = DataGridView1.Columns.Count - 1
        For i As Integer = 1 To headercount
            header += "," + DataGridView1.Columns(i).HeaderText
        Next
        streamwriter.WriteLine(header)
        Dim rowcount As Integer = DataGridView1.Rows.Count - 1
        For m As Integer = 0 To rowcount
            Dim strrowvalue As StringBuilder = New StringBuilder
            strrowvalue.Append(CStr(DataGridView1.Rows(m).Cells(0).Value))
            For n As Integer = 1 To DataGridView1.Columns.Count - 1
                If DataGridView1.CurrentCell.Value Is DBNull.Value Then
                    strrowvalue.Append("," + "0")
                End If
                If DataGridView1.Rows(m).Cells(n).Value Is DBNull.Value Then
                    strrowvalue.Append("," + "0")
                Else
                    strrowvalue.Append("," + CStr(DataGridView1.Rows(m).Cells(n).Value))
                End If
            Next
            strrowvalue = strrowvalue.Replace(vbCr, "")
            If m <> rowcount Then
                streamwriter.WriteLine(strrowvalue)
            End If
            strrowvalue.Clear()
        Next
        streamwriter.Close()
        ProgressBar1.Increment(1)
        If CheckBox1.Checked Then
            If str.Contains("Repeats_NonRepeats") Then
                If Not filedictionary.ContainsKey(CTdate) Then
                    filedictionary.Add(CTdate, New List(Of String)(New String() {str}))
                End If
                Dim dictkeys = filedictionary(CTdate)
                If Not dictkeys.Contains(str) Then
                    dictkeys.Add(str)
                End If
            End If
        End If
        If CheckBox2.Checked Then
            If str.Contains("Minutes") Then
                If Not filedictionary2.ContainsKey(CTdate) Then
                    filedictionary2.Add(CTdate, New List(Of String)(New String() {str}))
                End If
                Dim dictkeys = filedictionary2(CTdate)
                If Not dictkeys.Contains(str) Then
                    dictkeys.Add(str)
                End If
            End If
        End If
        If CheckBox5.Checked Then
            If Not rawfilelist.Contains(str) Then
                rawfilelist.Add(str)
            End If
        End If
        Return str
    End Function

    Private Function DiscoverDates(ByVal x As String)
        'This function loops through all the position files and creats a Dictionary that 
        'keeps track of dates each file contains.

        'This information is used by the PositionFileFixer to correctly build new position files that
        'are organzied by date and time.
        Dim line As String
        Dim initialdate As String
        For Each Me.posFilename In fd.FileNames
            filereader = My.Computer.FileSystem.OpenTextFileReader(Me.posFilename)
            Dim filename = Path.GetFileName(Me.posFilename)
            filenamelabel.Text = filename
            filenamelabel.Refresh()
            reportnamelabel.Text = "Discovering Dates"
            reportnamelabel.Refresh()
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
                            datedictionary.Add(initialdate, New List(Of String)(New String() {posFilename}))
                        End If
                        Dim dictkeys = datedictionary(initialdate)
                        If Not dictkeys.Contains(posFilename) Then
                            dictkeys.Add(posFilename)
                        End If
                    End If
                End If
            Loop
        Next
        ProgressBar1.Increment(1)
        Return True
    End Function

    Private Function CreatePositionFiles(ByVal x As Boolean)
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
        If x = True Then
            For Each strKey In datedictionary.Keys()
                filenamelabel.Text = Path.GetFileName(strKey)
                filenamelabel.Refresh()
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
                ProgressBar1.Increment(1)
            Next
            filereader.Close()
            filemaker.Close()
        ElseIf x = False Then
            For Each strKey In datedictionary.Keys()
                filenamelabel.Text = Path.GetFileName(strKey)
                filenamelabel.Refresh()
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
                'The following code may best be executed as a ForEach statement. It just deletes the files
                'that have no data in them. I tried a ForEach before, and it worked at first, then broke for some reason.
                'Fixing it isn't a 911.
                Dim h As New FileInfo(savefilename)
                If h.Length > 1 Then
                    filelist.Add(savefilename)
                ElseIf h.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename)
                End If
                Dim i As New FileInfo(savefilename2)
                If i.Length > 1 Then
                    filelist.Add(savefilename2)
                ElseIf i.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename2)
                End If
                Dim j As New FileInfo(savefilename3)
                If j.Length > 1 Then
                    filelist.Add(savefilename3)
                ElseIf j.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename3)
                End If
                Dim k As New FileInfo(savefilename4)
                If k.Length > 1 Then
                    filelist.Add(savefilename4)
                ElseIf k.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename4)
                End If
                Dim l As New FileInfo(savefilename5)
                If l.Length > 1 Then
                    filelist.Add(savefilename5)
                ElseIf l.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename5)
                End If
                Dim m As New FileInfo(savefilename6)
                If m.Length > 1 Then
                    filelist.Add(savefilename6)
                ElseIf m.Length < 1 Then
                    My.Computer.FileSystem.DeleteFile(savefilename6)
                End If
                ProgressBar1.Increment(1)
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

    Private Function CombineRawFileReports(ByVal x As Boolean)
        'This function takes all the RawFileReports generated by the program and combines their results into one file.
        Dim linesplit() As String
        Dim line As String
        reportnamelabel.Text = "Combining RAW File Reports"
        reportnamelabel.Refresh()
        Dim FullRawTable As DataTable = New DataTable("FullRaw")
        Dim tagid As DataColumn = New DataColumn()
        tagid.DataType = System.Type.GetType("System.String")
        tagid.ColumnName = "TagID"
        tagid.Unique = True
        FullRawTable.Columns.Add(tagid)
        Dim total As DataColumn = New DataColumn()
        total.DataType = System.Type.GetType("System.Int32")
        total.ColumnName = "Total"
        FullRawTable.Columns.Add(total)
        Dim zero As DataColumn = New DataColumn()
        zero.DataType = System.Type.GetType("System.Int32")
        zero.ColumnName = "Zero"
        FullRawTable.Columns.Add(zero)
        Dim one As DataColumn = New DataColumn()
        one.DataType = System.Type.GetType("System.Int32")
        one.ColumnName = "One"
        FullRawTable.Columns.Add(one)
        Dim two As DataColumn = New DataColumn()
        two.DataType = System.Type.GetType("System.Int32")
        two.ColumnName = "Two"
        FullRawTable.Columns.Add(two)
        Dim three As DataColumn = New DataColumn()
        three.DataType = System.Type.GetType("System.Int32")
        three.ColumnName = "Three"
        FullRawTable.Columns.Add(three)
        Dim four As DataColumn = New DataColumn()
        four.DataType = System.Type.GetType("System.Int32")
        four.ColumnName = "Four"
        FullRawTable.Columns.Add(four)
        Dim five As DataColumn = New DataColumn()
        five.DataType = System.Type.GetType("System.Int32")
        five.ColumnName = "five"
        FullRawTable.Columns.Add(five)
        Dim six As DataColumn = New DataColumn()
        six.DataType = System.Type.GetType("System.Int32")
        six.ColumnName = "six"
        FullRawTable.Columns.Add(six)
        Dim seven As DataColumn = New DataColumn()
        seven.DataType = System.Type.GetType("System.Int32")
        seven.ColumnName = "seven"
        FullRawTable.Columns.Add(seven)
        Dim eight As DataColumn = New DataColumn()
        eight.DataType = System.Type.GetType("System.Int32")
        eight.ColumnName = "eight"
        FullRawTable.Columns.Add(eight)
        Dim nine As DataColumn = New DataColumn()
        nine.DataType = System.Type.GetType("System.Int32")
        nine.ColumnName = "nine"
        FullRawTable.Columns.Add(nine)
        Dim more As DataColumn = New DataColumn()
        more.DataType = System.Type.GetType("System.Int32")
        more.ColumnName = "more"
        FullRawTable.Columns.Add(more)
        Dim good As DataColumn = New DataColumn()
        good.DataType = System.Type.GetType("System.Int32")
        good.ColumnName = "good"
        FullRawTable.Columns.Add(good)
        Dim bad As DataColumn = New DataColumn()
        bad.DataType = System.Type.GetType("System.Int32")
        bad.ColumnName = "bad"
        FullRawTable.Columns.Add(bad)
        Dim fulldaydata As DataSet = New DataSet()
        fulldaydata.Tables.Add(FullRawTable)
        Dim primarykeycolumns(0) As DataColumn
        primarykeycolumns(0) = FullRawTable.Columns("TagID")
        FullRawTable.PrimaryKey = primarykeycolumns
        Dim taglist As New List(Of String)
        For Each g In rawfilelist
            filereader = My.Computer.FileSystem.OpenTextFileReader(g)
            Do Until filereader.EndOfStream
                line = filereader.ReadLine()
                If line <> "" Then
                    linesplit = line.Split(New [Char]() {","})
                    If Char.IsLetter(linesplit(1)) = False Then
                        If Not taglist.Contains(linesplit(0)) Then
                            taglist.Add(linesplit(0))
                            Dim newrow As DataRow = FullRawTable.NewRow()
                            newrow(tagid) = linesplit(0)
                            newrow(total) = CInt(linesplit(1))
                            newrow(zero) = CInt(linesplit(2))
                            newrow(one) = CInt(linesplit(3))
                            newrow(two) = CInt(linesplit(4))
                            newrow(three) = CInt(linesplit(5))
                            newrow(four) = CInt(linesplit(6))
                            newrow(five) = CInt(linesplit(7))
                            newrow(six) = CInt(linesplit(8))
                            newrow(seven) = CInt(linesplit(9))
                            newrow(eight) = CInt(linesplit(10))
                            newrow(nine) = CInt(linesplit(11))
                            newrow(more) = CInt(linesplit(12))
                            newrow(good) = CInt(linesplit(13))
                            newrow(bad) = CInt(linesplit(14))
                            FullRawTable.Rows.Add(newrow)
                        ElseIf taglist.Contains(linesplit(0)) Then
                            foundrow = FullRawTable.Rows.Find(linesplit(0))
                            foundrow(1) += CInt(linesplit(1))
                            foundrow(2) += CInt(linesplit(2))
                            foundrow(3) += CInt(linesplit(3))
                            foundrow(4) += CInt(linesplit(4))
                            foundrow(5) += CInt(linesplit(5))
                            foundrow(6) += CInt(linesplit(6))
                            foundrow(7) += CInt(linesplit(7))
                            foundrow(8) += CInt(linesplit(8))
                            foundrow(9) += CInt(linesplit(9))
                            foundrow(10) += CInt(linesplit(10))
                            foundrow(11) += CInt(linesplit(11))
                            foundrow(12) += CInt(linesplit(12))
                            foundrow(13) += CInt(linesplit(13))
                            foundrow(14) += CInt(linesplit(14))
                        End If
                    End If
                End If
            Loop
        Next
        Try
            ds = New DataSet()
            ds = FullRawTable.DataSet
            DataGridView1.DataSource = ds.Tables(0)
            DataGridView1.Refresh()
        Catch ex2 As Exception
            MsgBox(ex2.ToString)
        End Try
        fullreportname = rawfilelist(0).Substring(0, rawfilelist(0).Length - 4)
        Dim FullReaderReport As String = WriteCSV(fullreportname, "FullReaderReport")
        Return True
    End Function

    Private Function CreateTables(ByVal cmdtxt As String, ByVal psnfile As String, ByVal firrow As String, ByVal ffile As String)
        'This function executes the SQL stored procedures that create the nessecary tables required by the reports selected.

        'Reminder: Position-based reports need to have the double quotes taken out from the position files before 
        '          tables can be created.
       
        Dim sql2 As New SqlCommand()
        Dim connectionstring As String = "Data Source=(local)\SQLExpress,34524;Initial Catalog=Analysis;Integrated Security=True;Connect Timeout=60"
        Dim connection As New SqlConnection(connectionstring)
        Try
            With sql2
                .CommandTimeout = 0
                .CommandText = cmdtxt
                .CommandType = CommandType.StoredProcedure
                If cmdtxt.Contains("Position") Then
                    .Parameters.Add("@positionfilepath", SqlDbType.NVarChar, 4000).Value = psnfile
                ElseIf cmdtxt.Contains("RAW") Then
                    .Parameters.Add("@rawfilepath", SqlDbType.NVarChar, 4000).Value = psnfile
                    .Parameters.Add("@firstrow", SqlDbType.NVarChar, 5).Value = firrow
                    .Parameters.Add("@formatfile", SqlDbType.NVarChar, 4000).Value = ffile
                End If
                .Connection = connection
                .Connection.Open()
                .ExecuteNonQuery()
                .Connection.Close()
                .Parameters.Clear()
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
        Return True
    End Function

    Private Function SQLCommand(ByVal cmdtxt As String, ByVal inttime As String, ByVal distance As String, ByVal sequenceregion As String)
        'This function executes the chosen reports by calling the corresponding SQL stored procedures.
        'The results are placed into the DataGridView, which is used to create .csv files for further analyzing.

        'The connectionstring variable needs to be adjusted to point to the user's local instance of SQL server.
        Dim sql2 As New SqlCommand()
        Dim connectionstring As String = "Data Source=(local)\SQLExpress,34524;Initial Catalog=Analysis;Integrated Security=True;Connect Timeout=60"
        Dim connection As New SqlConnection(connectionstring)
        Try
            sql2 = New SqlCommand
            With sql2
                .CommandTimeout = 0
                .CommandText = cmdtxt
                If cmdtxt.Contains("RepeatsPerXMinutes") Then
                    .Parameters.Add("@intervaltime", SqlDbType.NVarChar, 30).Value = inttime
                ElseIf cmdtxt.Contains("CoordinateJumpCount") Then
                    .Parameters.Add("@JumpDistance", SqlDbType.NVarChar, 10).Value = distance
                ElseIf cmdtxt.Contains("DroppedPackets") Then
                    .Parameters.Add("@sequenceNumRegion", SqlDbType.NVarChar, 2).Value = sequenceregion
                End If
                .CommandType = CommandType.StoredProcedure
                .Connection = connection
                .Connection.Open()
            End With
            Dim sda As New SqlDataAdapter(sql2)
            Try
                ds = New DataSet()
                sda.Fill(ds)
                DataGridView1.DataSource = ds.Tables(0)
                DataGridView1.Refresh()
            Catch ex2 As Exception
                MsgBox(ex2.ToString)
            End Try
        Catch ex1 As Exception
            MsgBox(ex1.ToString)
        End Try
        sql2.Parameters.Clear()
        sql2.Connection.Close()

        Return True
    End Function

    Private Function TotalCTReport(ByVal x As String)
        'This function combines the 4-hour CT reports from a single day into a full-day report.
        'It creates a table in the DataGridView, then calculates the numbers per assetID.

        'This function also generates a full-day report of RepeatsPerXMinutes. It takes all the 4-hour
        'reports of that type for a day and appends them together.

        'It will create a Full-Day Report if there is more than 1 position file processed per given day.
        'Possible tweak: Adjust report names to indicate whether the reports actually covered a full 24-hour period, 
        '                or were shorter amounts of time.

        'TO DO: Run these reports after the last file for a day is processed. Right now, these reports only run when EVERY
        '       FILE in the queue has been processed.

        Dim line As String
        Dim linesplit() As String
        reportnamelabel.Text = "Processing Full Day Results"
        reportnamelabel.Refresh()
        Dim FullDayTable As DataTable = New DataTable("FullDay")
        Dim assetid As DataColumn = New DataColumn()
        assetid.DataType = System.Type.GetType("System.String")
        assetid.ColumnName = "AssetId"
        assetid.Unique = True
        FullDayTable.Columns.Add(assetid)
        Dim tagid As DataColumn = New DataColumn()
        tagid.DataType = System.Type.GetType("System.String")
        tagid.ColumnName = "TagId"
        tagid.Unique = False
        FullDayTable.Columns.Add(tagid)
        Dim repeats As DataColumn = New DataColumn()
        repeats.DataType = System.Type.GetType("System.Int32")
        repeats.ColumnName = "Repeats"
        FullDayTable.Columns.Add(repeats)
        Dim nonrepeats As DataColumn = New DataColumn()
        nonrepeats.DataType = System.Type.GetType("System.Int32")
        nonrepeats.ColumnName = "Non_Repeats"
        FullDayTable.Columns.Add(nonrepeats)
        Dim percentofrepeats As DataColumn = New DataColumn()
        percentofrepeats.DataType = System.Type.GetType("System.Decimal")
        percentofrepeats.ColumnName = "Percent_of_Repeats"
        FullDayTable.Columns.Add(percentofrepeats)
        Dim CT As DataColumn = New DataColumn()
        CT.DataType = System.Type.GetType("System.Int32")
        CT.ColumnName = "Repeats_Resulting_in_CT"
        FullDayTable.Columns.Add(CT)
        Dim percentofCT As DataColumn = New DataColumn()
        percentofCT.DataType = System.Type.GetType("System.Decimal")
        percentofCT.ColumnName = "Percent_of_CT"
        FullDayTable.Columns.Add(percentofCT)
        Dim NCT As DataColumn = New DataColumn()
        NCT.DataType = System.Type.GetType("System.Int32")
        NCT.ColumnName = "Repeats_Not_Resulting_in_CT"
        FullDayTable.Columns.Add(NCT)
        Dim percentofNCT As DataColumn = New DataColumn()
        percentofNCT.DataType = System.Type.GetType("System.Decimal")
        percentofNCT.ColumnName = "Percent_of_NCT"
        FullDayTable.Columns.Add(percentofNCT)
        Dim fulldaydata As DataSet = New DataSet()
        fulldaydata.Tables.Add(FullDayTable)
        Dim primarykeycolumns(0) As DataColumn
        primarykeycolumns(0) = FullDayTable.Columns("AssetId")
        FullDayTable.PrimaryKey = primarykeycolumns
        For Each strKey In filedictionary.Keys()
            Dim keylist = filedictionary(strKey)
            Dim assetlist As New List(Of String)
            For Each strfilename In keylist
                filereader = My.Computer.FileSystem.OpenTextFileReader(strfilename)
                fullreportname = strfilename
                Do Until filereader.EndOfStream
                    line = filereader.ReadLine()
                    If line <> "" Then
                        linesplit = line.Split(New [Char]() {","})
                        If Char.IsLetter(linesplit(1)) = False Then
                            If Not assetlist.Contains(linesplit(0)) Then
                                assetlist.Add(linesplit(0))
                                Dim newrow As DataRow = FullDayTable.NewRow()
                                newrow(assetid) = linesplit(0)
                                newrow(tagid) = linesplit(1)
                                newrow(repeats) = CInt(linesplit(2))
                                newrow(nonrepeats) = CInt(linesplit(3))
                                newrow(percentofrepeats) = Math.Round(1 - (CDec(linesplit(3)) / (CDec((linesplit(2)) + CDec(linesplit(3))))), 4)
                                newrow(CT) = CInt(linesplit(5))
                                If CInt(linesplit(2)) <> 0 Then
                                    newrow(percentofCT) = Math.Round(CDec(linesplit(5)) / CDec(linesplit(2)), 4)
                                ElseIf CInt(linesplit(2)) = 0 Then
                                    newrow(percentofCT) = 0
                                End If
                                newrow(NCT) = CInt(linesplit(7))
                                If CInt(linesplit(2)) <> 0 Then
                                    newrow(percentofNCT) = Math.Round(CDec(linesplit(7)) / CDec(linesplit(2)), 4)
                                ElseIf CInt(linesplit(2)) = 0 Then
                                    newrow(percentofNCT) = 0
                                End If
                                FullDayTable.Rows.Add(newrow)
                                totalrepeats += newrow(2)
                                totalnonrepeats += newrow(3)
                                totalCT += newrow(5)
                                If newrow(4) > 0.2 Then
                                    If Not tagerrordictionary.Keys.Contains(newrow(0)) Then
                                        tagerrordictionary.Add(newrow(0), newrow(4))
                                    ElseIf tagerrordictionary(newrow(assetid)) < newrow(4) Then
                                        tagerrordictionary(newrow(assetid)) = newrow(4)
                                    End If
                                End If
                                fulldayrepeats += newrow(2)
                                fulldaynonrepeats += newrow(3)
                                fulldayct += newrow(5)
                            ElseIf assetlist.Contains(linesplit(0)) Then
                                foundrow = FullDayTable.Rows.Find(linesplit(0))
                                foundrow(2) += CInt(linesplit(2))
                                foundrow(3) += CInt(linesplit(3))
                                If CInt(foundrow(2)) <> 0 Then
                                    foundrow(4) = Math.Round(CDec(foundrow(2)) / (CDec(foundrow(2) + CDec(foundrow(3)))), 4)
                                ElseIf CInt(foundrow(2)) = 0 Then
                                    foundrow(4) = 0
                                End If
                                foundrow(5) += CInt(linesplit(5))
                                If CInt(foundrow(2)) <> 0 Then
                                    foundrow(6) = Math.Round(CDec(foundrow(5)) / CDec(foundrow(2)), 4)
                                ElseIf CInt(foundrow(2)) = 0 Then
                                    foundrow(6) = 0
                                End If
                                foundrow(7) += CInt(linesplit(7))
                                If CInt(foundrow(2)) <> 0 Then
                                    foundrow(8) = Math.Round(CDec(foundrow(7)) / CDec(foundrow(2)), 4)
                                ElseIf CInt(foundrow(2)) = 0 Then
                                    foundrow(8) = 0
                                End If
                                totalrepeats += foundrow(2)
                                totalnonrepeats += foundrow(3)
                                totalCT += foundrow(5)
                                fulldayrepeats += foundrow(2)
                                fulldaynonrepeats += foundrow(3)
                                fulldayct += foundrow(5)
                            End If
                        End If
                    End If
                Loop
                If totalrepeats <> 0 Then
                    CTpercent = Math.Round(CDec((totalnonrepeats + totalCT) / (totalrepeats + totalnonrepeats)), 4) * 100
                    ctdictionary.Add(strfilename, CTpercent)
                    totalCT = 0
                    totalnonrepeats = 0
                    totalrepeats = 0
                End If
            Next
            Try
                ds = New DataSet()
                ds = FullDayTable.DataSet
                DataGridView1.DataSource = ds.Tables(0)
                DataGridView1.Refresh()
            Catch ex2 As Exception
                MsgBox(ex2.ToString)
            End Try
            Dim FulldayReport As String = WriteCSV(fullreportname.Substring(0, fullreportname.Length - 29), "FullDayReport")
            numberofassets = assetlist.Count
            CTpercent = Math.Round(CDec((fulldaynonrepeats + fulldayct) / (fulldayrepeats + fulldaynonrepeats)), 4) * 100
            If Not ctdictionary.ContainsKey(FulldayReport) Then
                ctdictionary.Add(FulldayReport, CTpercent)
            End If
            FullDayTable.Clear()
        Next

        For Each strKey In filedictionary2.Keys()
            Dim str As String = saveFileLocation + strKey + "FullDayRepeats.csv"
            Dim keylist = filedictionary2(strKey)
            Dim assetlist As New List(Of String)
            Dim streamwriter As New System.IO.StreamWriter(str)
            Dim headercounter As Int16 = 0
            For Each strfilename In keylist
                filereader = My.Computer.FileSystem.OpenTextFileReader(strfilename)
                fullreportname = strfilename
                Do Until filereader.EndOfStream
                    line = filereader.ReadLine()
                    If line.Contains("T") Then
                        If headercounter < 1 Then
                            streamwriter.WriteLine(line)
                            headercounter += 1
                        End If
                    Else
                        streamwriter.WriteLine(line)
                    End If
                Loop
            Next
            filereader.Close()
            streamwriter.Close()
        Next

        'This bit wraps up the results of the RepeatsNonRepeats report into a quick summary for the user.
        'The user sees the contigual track percent for each 4-hour segment, and the contigual track for the 24-hour day
        'as a whole.
        'Any asset that reported repeat percentages over 20% at any point are also displayed with their repeat percentage.

        'TO DO: Place summary data in a scrollable box. Right now the data is shown in a MsgBox that goes off the edge
        '       of the screen and it can only be closed by clicking the red "X" in the corner.


        My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
        Dim reportbuilder As New StringBuilder()
        If ctdictionary.Count > 0 Then
            For Each strKey In ctdictionary.Keys()
                Dim keylist = ctdictionary(strKey)
                If strKey.Contains("FullDay") Then
                    reportbuilder.AppendLine("File " + Path.GetFileName(strKey) + " had a CT of " + CStr(keylist) + "%")
                Else
                    reportbuilder.AppendLine("File " + Path.GetFileName(strKey).Substring(Path.GetFileName(strKey).Length - 22) + " had a CT of " + CStr(keylist) + "%")
                End If
            Next
            MsgBox(reportbuilder.ToString(), , "Summary")
        End If

        Return True
    End Function

    Private Function LoggingService(ByVal rawfile As String)
        'This function activates the logging service after it updates the config file with chosen save locations and report types.
        'The function will automatically shut off the logging service when it finishes processing a file, then restarts it
        'with the next file in the queue. 
        'This stopping and starting is nessecary to avoid the backing up of positions in the logging service, which 
        'results in non-stop repeated positions after about 3 days worth of RAW files have been processed.

        'TO DO: Maybe add a pop-up message reminding the user to clear out any position tables already in MySQL.
        '       A clean database makes it easier to work with the tables the Logging Service creates.
        'TO DO #2: Work with this code to find out why it is stopping before all positions have been written out to 
        '          zip files. It's a pain to have to manually extract the data from MySQL tables.

        reportnamelabel.Text = "Running the Logging Service"
        reportnamelabel.Refresh()
        '*************Ensure appsettings variable is set to correct file location to read the config file***************
        Dim appsettings() As String = File.ReadAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config")
        appsettings(7) = "  <add key=" + """TrackerDataSourceFile""" + " value=" + """" + rawfile + """" + "/>"

        '*************Make sure these directories are correct************************
        If RadioButton1.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\TEST CONFIG SPEED 0" + "\trackerconfig.txt" + """" + "/>"
        ElseIf RadioButton2.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\TEST LEFT w ALL READERS AND SPEED 0" + "\trackerconfig.txt" + """" + "/>"
        ElseIf RadioButton3.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\Both tags 2 readers removed" + "\trackerconfig.txt" + """" + "/>"
        ElseIf RadioButton4.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\Left tag only 2 readers removed" + "\trackerconfig.txt" + """" + "/>"
        End If

        File.WriteAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config", appsettings)

        '****************point this to the correct location for the logging service.exe******************

        Dim runningprocess As Process = Process.Start("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\Plus.LoggingService.exe")

        Do Until runningprocess.HasExited
            Threading.Thread.Sleep(1000)
        Loop

        Return True



    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        'This function passes the SQL Stored Procedure commands to CreateTables and SQLCommand functions.
        'The command passed is based on the report chosen, obviously.
        'Before any command is executed, the position or RAW data needs to be loaded up into a table in SQL.

        'After commands are run, the WriteCSV function is called and the results are saved into the indicated folder.
        'Once all reports have finished the application indicates completion by making the windows "Asterisk" sound
        'and displaying a "Reports Complete" message. It also sets all variables to empty and unchecked.

        'TO DO: Combine the reports complete message with the bad tag message (assets > 20% repeats) and contigual
        'track messages. Make it scrollable. Maybe have tabs that contain each type of data.

        Dim distance As String
        Dim sequenceregion As String
        If saveFileLocation <> "" Then

            If fd2.FileNames.Count > 1 AndAlso CheckBox8.Checked = False Then
                If MsgBox("Do you want to combine the results into one file?", vbQuestion + vbYesNo) = vbYes Then
                    combinereports = True
                End If
                If CheckBox6.Checked = True Then
                    sequenceregion = InputBox("Client location number. Type 1 for USA, or 2 for other.", "Region Entry", , 250, 75)
                    If sequenceregion = "" Then
                        sequenceregion = "1"
                    End If
                End If
            End If

            If posFilename <> "" Then
                If MsgBox("Do position files need to be reworked?", vbQuestion + vbYesNo) = vbNo Then
                    processpositionfiles = False
                    For Each posFilename As String In fd.FileNames
                        filelist.Add(posFilename)
                    Next
                End If

                If processpositionfiles = True Then
                    If MsgBox("Do you want to run the chosen reports over a 24 hour period of data? Default is 4 hour periods." & vbCrLf & "WARNING: Using 24-hour files for reports 1,4, or 5 can result in extremely long processing times!", vbQuestion + vbYesNo) = vbYes Then
                        reportduration24 = True
                    End If
                End If

                If CheckBox2.Checked = True Then
                    interval = InputBox("Input time in minutes", "Time Entry", , 250, 75)
                End If
                If CheckBox3.Checked = True Then
                    distance = InputBox("Input Distance in Meters", "Distance Entry", , 250, 75)
                End If

                If processpositionfiles = True Then
                    Dim tablemaker As Boolean = DiscoverDates(posFilename)
                    If reportduration24 = True Then
                        reportnamelabel.Text = "Creating 24-hour Position file"
                        reportnamelabel.Refresh()
                        Dim report24 As Boolean = CreatePositionFiles(True)
                    ElseIf reportduration24 = False Then
                        reportnamelabel.Text = "Creating 4-hour position files"
                        reportnamelabel.Refresh()
                        Dim report4 As Boolean = CreatePositionFiles(False)
                    End If
                    ProgressBar1.Increment(1)
                End If

                Dim makethebar As Boolean = buildprogressbar(True)
                For Each g In filelist
                    filenamelabel.Text = Path.GetFileName(g)
                    filenamelabel.Refresh()
                    Try
                        reportnamelabel.Text = "Creating Position Table"
                        reportnamelabel.Refresh()
                        Dim tablebuilder As Boolean = CreateTables("PositionDataInsertion", g, "", "")
                        ProgressBar1.Increment(1)
                    Catch ex As Exception
                        MsgBox(ex.ToString)
                    End Try
                    If CheckBox1.Checked = True Or CheckBox4.Checked = True Or CheckBox7.Checked = True Then
                        Try
                            reportnamelabel.Text = "Creating Temp Table"
                            reportnamelabel.Refresh()
                            Dim tablebuilder As Boolean = CreateTables("TempTableCreate", "", "", "")
                            ProgressBar1.Increment(1)
                        Catch ex As Exception
                            MsgBox(ex.ToString)
                        End Try
                    End If
                    If CheckBox1.Checked = True Then
                        reportnamelabel.Text = "Processing Report #1"
                        reportnamelabel.Refresh()
                        Dim reportrun As Boolean = SQLCommand("Repeats_NonRepeats", "", "", "")
                        ProgressBar1.Increment(1)
                        Dim reportcomplete As String = WriteCSV(g.Substring(0, g.Length - 4), "Repeats_NonRepeats")
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox2.Checked = True Then
                        reportnamelabel.Text = "Processing Report #2"
                        reportnamelabel.Refresh()
                        If interval = "" Then
                            MsgBox("You must enter an interval length in minutes")
                        ElseIf interval <> "" Then
                            Dim reportrun As Boolean = SQLCommand("RepeatsPerXMinutes", interval, "", "")
                            Dim reportcomplete As String = WriteCSV(g.Substring(0, g.Length - 4), "Repeats_Per" + interval + "Minutes")
                        End If
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox3.Checked = True Then
                        reportnamelabel.Text = "Processing Report #3"
                        reportnamelabel.Refresh()
                        If distance = "" Then
                            MsgBox("Error: Distance not entered")
                        ElseIf distance <> "" Then
                            Dim reportrun As Boolean = SQLCommand("CoordinateJumpCount", "", distance, "")
                            Dim reportcomplete As String = WriteCSV(g.Substring(0, g.Length - 4), "CoordinateJumpCount")
                        End If
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox4.Checked Then
                        reportnamelabel.Text = "Processing Report #4"
                        reportnamelabel.Refresh()
                        Dim reportrun As Boolean = SQLCommand("RepeatedIntervals", "", "", "")
                        Dim reportcomplete As String = WriteCSV(g.Substring(0, g.Length - 4), "RepeatedIntervals")
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox7.Checked = True Then
                        reportnamelabel.Text = "Processing Report #5"
                        reportnamelabel.Refresh()
                        Dim reportrun As Boolean = SQLCommand("RepeatCoordinateJumpAnalysis", "", "", "")
                        Dim reportcomplete As String = WriteCSV(g.Substring(0, g.Length - 4), "RepeatedJumpAnalysis")
                        ProgressBar1.Increment(1)
                    End If
                Next
            End If
            If rawFilename <> "" Then
                Dim rawfilefirstrow As String
                Dim formatfile As String
                If CheckBox5.Checked = True Or CheckBox6.Checked = True Then
                    rawfilefirstrow = InputBox("Row number in the file that the reader data begins on. Default row is 404,", "Row Number Entry", , 250, 75)
                    formatfile = InputBox("Location of format file used to correctly align the data for table creation.", "Format File Entry", , 250, 75)
                    If rawfilefirstrow = "" Then
                        rawfilefirstrow = "404"
                    End If
                    If formatfile = "" Then
                        formatfile = "C:\Users\jsharitt\Desktop\ForJason\Trial3\RawDataBulkInsertConfig.txt"
                    End If
                End If
                For Each Me.rawFilename In fd2.FileNames
                    filenamelabel.Text = Path.GetFileName(Me.rawFilename)
                    filenamelabel.Refresh()
                    If CheckBox6.Checked = True Or CheckBox5.Checked = True Then
                        Try
                            reportnamelabel.Text = "Creating Raw Table"
                            reportnamelabel.Refresh()
                            Dim tablebuilder As Boolean = CreateTables("RAWDataInsertion", rawFilename, rawfilefirstrow, formatfile)
                        Catch ex As Exception
                            MsgBox(ex.ToString)
                        End Try
                    End If
                    If CheckBox5.Checked = True Then
                        reportnamelabel.Text = "Processing Report #6"
                        reportnamelabel.Refresh()
                        Dim reportrun As Boolean = SQLCommand("ReaderAnalysis", "", "", "")
                        Dim reportcomplete As String = WriteCSV(rawFilename.Substring(0, rawFilename.Length - 4), "ReaderAnalysis")
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox6.Checked = True Then
                        reportnamelabel.Text = "Processing Report #7"
                        reportnamelabel.Refresh()
                        Dim reportrun As Boolean = SQLCommand("DroppedPackets", "", "", "")
                        Dim reportcomplete As String = WriteCSV(rawFilename.Substring(0, rawFilename.Length - 4), "DroppedPackets")
                        ProgressBar1.Increment(1)
                    End If
                    If CheckBox8.Checked = True Then
                        If RadioButton1.Checked = False AndAlso RadioButton2.Checked = False AndAlso RadioButton3.Checked = False AndAlso RadioButton4.Checked = False Then
                            MsgBox("No Config Selected", , "ERROR")
                        Else
                            Dim createfiles As Boolean = LoggingService(rawFilename)
                            If createfiles = False Then
                                MsgBox("Logging Service did not start successfully!")
                            End If
                        End If
                        ProgressBar1.Increment(1)
                    End If
                Next
            End If
            If combinereports = True Then
                Dim doreports As Boolean = CombineRawFileReports(True)
            End If
            If CheckBox1.Checked = True And filelist.Count > 1 Then
                Dim finalreport As Boolean = TotalCTReport("x")
                ProgressBar1.Increment(1)
            End If


            If tagerrordictionary.Count > 0 Then
                Dim errorlist As New StringBuilder()
                For Each assetid In tagerrordictionary.Keys()
                    Dim keylist = tagerrordictionary(assetid)
                    errorlist.AppendLine("Asset ID " + assetid + " returned " + CStr(keylist * 100) + "% repeated positions")
                Next
                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
                MsgBox(errorlist.ToString(), , "ERRORS")
            End If
            If tagerrordictionary.Count < 1 AndAlso CheckBox1.Checked Then
                MsgBox("There were no tags with more than 20% repeated positions", , "No Tag Errors Found")
            End If
            MsgBox("All Reports Complete", , "Reports Complete")
            reportnamelabel.Text = "Reports Complete"
            reportnamelabel.Refresh()
            filenamelabel.Text = ""
            filenamelabel.Refresh()
            CheckBox1.Checked = False
            CheckBox2.Checked = False
            CheckBox3.Checked = False
            CheckBox4.Checked = False
            CheckBox5.Checked = False
            CheckBox6.Checked = False
            CheckBox7.Checked = False
            CheckBox8.Checked = False
            ProgressBar1.Value = 0
            ListView1.Clear()
            ListView2.Clear()
            posFilename = ""
            rawFilename = ""
            filelist.Clear()
        ElseIf saveFileLocation = "" Then
            MsgBox("Save File Location not chosen")
        End If

    End Sub

    Private Sub RadioButton4_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton4.CheckedChanged

    End Sub
End Class
