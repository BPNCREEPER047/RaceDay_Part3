# RaceDay – API Endpoint Plan
**Part 1 – Section B**

> This table lists every API endpoint the RaceDay system will expose in Part 2.
> All routes begin with `/api/`. Role options: None (public), Any (logged in), Organiser, Participant.

---

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/auth/register | Registers a new user account (Organiser or Participant) | None | `{ fullName, email, password, role }` | 201 Created – user object returned; 400 Bad Request – missing fields; 409 Conflict – email already in use |
| POST | /api/auth/login | Logs in an existing user and returns a JWT token | None | `{ email, password }` | 200 OK – `{ token, user }`; 400 Bad Request – missing fields; 401 Unauthorized – wrong credentials |

---

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/users/profile | Returns the profile of the currently logged-in user | Any | None | 200 OK – user object; 401 Unauthorized – not logged in |
| PUT | /api/users/profile | Updates the currently logged-in user's profile details | Any | `{ fullName, email, password }` | 200 OK – updated user; 400 Bad Request – invalid data; 401 Unauthorized |
| GET | /api/users | Returns a list of all users in the system | Organiser | None | 200 OK – array of users; 401 Unauthorized; 403 Forbidden – not an Organiser |

---

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/categories | Returns all event categories | None | None | 200 OK – array of categories |
| GET | /api/categories/{id} | Returns a single category by ID | None | None | 200 OK – category object; 404 Not Found – category does not exist |
| POST | /api/categories | Creates a new event category | Organiser | `{ categoryName, description }` | 201 Created – new category; 400 Bad Request – missing fields; 401 Unauthorized; 403 Forbidden |
| PUT | /api/categories/{id} | Updates an existing category by ID | Organiser | `{ categoryName, description }` | 200 OK – updated category; 404 Not Found; 403 Forbidden |
| DELETE | /api/categories/{id} | Deletes a category by ID | Organiser | None | 200 OK – success message; 404 Not Found; 403 Forbidden |

---

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/events | Returns all upcoming events (browsable by participants) | None | None | 200 OK – array of events |
| GET | /api/events/{id} | Returns a single event by ID including route info | None | None | 200 OK – event object; 404 Not Found |
| POST | /api/events | Creates a new event | Organiser | `{ eventName, description, eventDate, location, distance, maxParticipants, categoryID }` | 201 Created – new event; 400 Bad Request; 401 Unauthorized; 403 Forbidden |
| PUT | /api/events/{id} | Updates an existing event by ID | Organiser | `{ eventName, description, eventDate, location, distance, maxParticipants, categoryID }` | 200 OK – updated event; 404 Not Found; 403 Forbidden |
| DELETE | /api/events/{id} | Deletes an event by ID | Organiser | None | 200 OK – success message; 404 Not Found; 403 Forbidden |
| GET | /api/events/{id}/enrolments | Returns all enrolments for a specific event | Organiser | None | 200 OK – array of enrolments; 404 Not Found; 403 Forbidden |

---

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/enrolments | Returns all enrolments for the logged-in participant | Participant | None | 200 OK – array of enrolments; 401 Unauthorized; 403 Forbidden |
| POST | /api/enrolments | Enrols the logged-in participant into an event | Participant | `{ eventID }` | 201 Created – enrolment record; 400 Bad Request – missing eventID; 404 Not Found – event does not exist; 409 Conflict – already enrolled |
| DELETE | /api/enrolments/{id} | Cancels/removes a specific enrolment | Participant | None | 200 OK – success message; 404 Not Found – enrolment does not exist; 403 Forbidden – not your enrolment |

---

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/results | Returns all results for the logged-in participant's enrolments | Participant | None | 200 OK – array of results; 401 Unauthorized |
| GET | /api/results/event/{eventId} | Returns all results for a specific event | Organiser | None | 200 OK – array of results; 404 Not Found; 403 Forbidden |
| POST | /api/results | Captures a result for a participant's enrolment | Organiser | `{ enrolmentID, finishTime, position, notes }` | 201 Created – result object; 400 Bad Request; 404 Not Found – enrolment not found; 409 Conflict – result already exists |
| PUT | /api/results/{id} | Updates an existing result | Organiser | `{ finishTime, position, notes }` | 200 OK – updated result; 404 Not Found; 403 Forbidden |
| DELETE | /api/results/{id} | Deletes a result record | Organiser | None | 200 OK – success message; 404 Not Found; 403 Forbidden |

---

## Routes (Race Routes)

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/routes/{eventId} | Returns route information for a specific event | None | None | 200 OK – route object; 404 Not Found – no route for this event |
| POST | /api/routes | Adds route information for an event | Organiser | `{ eventID, routeMapURL, startPoint, endPoint, elevationGain, description }` | 201 Created – route object; 400 Bad Request; 403 Forbidden; 409 Conflict – route already exists for event |
| PUT | /api/routes/{eventId} | Updates route information for an event | Organiser | `{ routeMapURL, startPoint, endPoint, elevationGain, description }` | 200 OK – updated route; 404 Not Found; 403 Forbidden |

---

*Total endpoints: 25*
*This plan will be implemented in Part 2 using C# RESTful API.*
