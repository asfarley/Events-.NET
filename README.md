# Events .NET
Events .NET is a Windows WPF GUI application for visualizing event-stream histograms. Just drop your CSV file containing events onto the UI. 

The application looks for events in the following format:

```
23/05/22 17:01:33 UTC,P2,P1,Bicycle,Straight  
23/05/22 17:01:36 UTC,P2,P1,Pedestrian,Straight  
23/05/22 17:01:44 UTC,P2,P1,Pedestrian,Straight  
```

Where the first element is a timestamp, followed by a list of strings. 
