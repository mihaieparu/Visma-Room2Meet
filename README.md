##Welcome to the Visma Room2Meet application!

The main application can be found at **http://room2meet.azurewebsites.net/**.

Warning! All dates are converted to the UTC format! Please keep in mind that Romania's hour is UTC+2 (during the winter)/UTC+3 (during the summer).

##Website documentation

###User types
The application supports 4 types of users:
* Administrator (code -1) (is allowed to create, edit, remove all the entities - users, buildings, rooms, assets, bookings)
* Executive (code 2) (represents the highest level a simple user could become. is allowed to change only data created by users with a code lower than his or by him)
* Team leader (code 1) (represents the middle level a simple user could become. is allowed to change only data created by users with a code lower than his or by him)
* Employee (code 0) (represents the simple user. is allowed to change only data created by him)

###Entities
Although the initial challenge was to book rooms only from one building, I went even further and created a new entity object - the building, because Visma is an international company with a lot of buildings. So the system is applicable in any of these buildings.

####Users
For managing the users, I have used the standard ASP.NET identity system, with some small changes (added the role order, user name, phone and building).
When a simple user registers, it becomes locked out, waiting to be approved. Only administrators can approve new users on the platform.

####Building
The building entity has a unique code, which is used mainly for display purposes, a country field, address (which accepts HTML tags like br or hr), and also an image URL (which must be external. although, this isn't required, but it's a nice feature). Also, you can set the open and closing hour, in order to prevent bookings outside the working schedule.

####Room
The room must be attached to a building. It has to have a floor value set, a capacity and also a room code + image URL. More important, you can set the default booking status when a new booking is created, the maximum booking hours allowed, the assets for the room and also the user types which can book the room

####Asset
The asset represents a general thing which can be found in a room (eg. projector, TV etc.). Once it is added to a room, you can say in the description of the RoomAsset which model, type or the size of the asset.

####Booking
Once you are booking a room, only you or users with a higher code can update your booking. What's more, you'll have a booking reference which you should note down in the future and a status which you should update when the meeting begins or is completed. Also, any change for any booking is kept in a booking history. If the user who made the change is different than the user who created the booking, a SMS will be sent to the user who created it in order to announce the event so that he will review the change. I have used the Twilio API for this feature.
In the future, I want to make a different application for the meeting rooms, in which you should insert the booking reference or scan it from the QR code and your account's password in order to access the room.

####Logs & misc
I have made a logging system based on normal and custom exceptions. The logs are saved in the database, and then can be queried in order to see which were the problems encountered by the users.

##Authentication
* Administrator: **admin@visma.ro**
* Executive user: **executive@visma.ro**
* TeamLeader user: **teamleader@visma.ro**
* Employee: **employee@visma.ro**

For all the users, the password is **CODEFORCASH**
