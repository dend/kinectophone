# KinectoPhone

KinectoPhone is a project developed for the Kinect for Windows SDK. [Adam Kinney](https://twitter.com/adkinn), [Rick Barazza](https://twitter.com/rickbarraza) and [Dennis Delimarsky](https://twitter.com/denniscode) built a prototype, showing the possibilities of integrating a Kinect-powered desktop application with a Windows Phone 7 application by providing a co-op gaming experience.

![Screenshot of the KinectoPhone project](media/phone-screenshot.png)

We used skeletal tracking to manipulate the main character in the WPF-based application and sockets to communicate with the mobile layer (the Windows Phone application) because of serious performance lags present with a WCF service being the connection point.

## Article

You can read more about the project [on Channel9](https://channel9.msdn.com/coding4fun/articles/kinectophone).
