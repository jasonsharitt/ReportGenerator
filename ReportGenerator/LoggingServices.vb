Imports System.IO

Public Class LoggingServices
    Public Function LoggingService(ByVal rawfile As String)
        'This function activates the logging service after it updates the config file with chosen save locations and report types.
        'The function will automatically shut off the logging service when it finishes processing a file, then restarts it
        'with the next file in the queue. 
        'This stopping and starting is nessecary to avoid the backing up of positions in the logging service, which 
        'results in non-stop repeated positions after about 3 days worth of RAW files have been processed.

        'TO DO: Maybe add a pop-up message reminding the user to clear out any position tables already in MySQL.
        '       A clean database makes it easier to work with the tables the Logging Service creates.
        'TO DO #2: Work with this code to find out why it is stopping before all positions have been written out to 
        '          zip files. It's a pain to have to manually extract the data from MySQL tables.

        '*************Ensure appsettings variable is set to correct file location to read the config file***************
        Dim appsettings() As String = File.ReadAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config")
        appsettings(7) = "  <add key=" + """TrackerDataSourceFile""" + " value=" + """" + rawfile + """" + "/>"

        '*************Make sure these directories are correct************************
        If Form1.RadioButton1.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\TEST CONFIG SPEED 0" + "\trackerconfig.txt" + """" + "/>"
        ElseIf Form1.RadioButton2.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\TEST LEFT w ALL READERS AND SPEED 0" + "\trackerconfig.txt" + """" + "/>"
        ElseIf Form1.RadioButton3.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\Both tags 2 readers removed" + "\trackerconfig.txt" + """" + "/>"
        ElseIf Form1.RadioButton4.Checked = True Then
            appsettings(6) = "  <add key=" + """TrackerConfig""" + " value=" + """" + "C:\Program Files (x86)\PLUS Location Systems\PLUS Logging Service\config\Left tag only 2 readers removed" + "\trackerconfig.txt" + """" + "/>"
        End If

        File.WriteAllLines("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\config\appSettings.config", appsettings)

        '****************point this to the correct location for the logging service.exe******************

        Dim runningprocess As Process = Process.Start("C:\Repos\dls\tags\1.0.4\Plus.LoggingService\bin\Debug\Plus.LoggingService.exe")

        Do Until runningprocess.HasExited
            Threading.Thread.Sleep(10000)
        Loop

        Return True



    End Function
End Class
