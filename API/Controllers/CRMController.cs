using Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMController : ControllerBase
    {

        [HttpGet("LoginUser")]
        public IActionResult LoginUser([FromQuery] User user)
        {
            string loginMessage = null;
            bool success = false;
            int userId = 0; // Declare userId outside the loop

            // Your existing SQL query with string interpolation
            string query = $"SELECT CASE WHEN Username = '{user.Username}' AND Password = '{user.Password}' THEN 'Login successful' WHEN Username = '{user.Username}' THEN 'Password is incorrect' ELSE 'Username not found' END AS login_message, CASE WHEN Username = '{user.Username}' AND Password = '{user.Password}' THEN UserID ELSE NULL END AS UserID FROM Users WHERE Username = '{user.Username}'";

            using (var connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CRMDB;Integrated Security=True;"))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();

                // Use ExecuteReader to get the login_message result
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loginMessage = reader["login_message"].ToString();

                        // Check if UserID is not null before reading
                        if (!reader.IsDBNull(reader.GetOrdinal("UserID")))
                        {
                            userId = reader.GetInt32(reader.GetOrdinal("UserID"));
                        }

                        // Set success flag based on the login message
                        if (loginMessage == "Login successful")
                        {
                            success = true; // Login is successful 
                        }
                    }
                }
            }

            // Return the result to the client
            return Ok(new { success = success, message = loginMessage, userId = userId });
        }


        [HttpGet("GetUserCompanies")]
        public IActionResult GetUserCompanies(int userId)
        {
            // SQL query with string interpolation
            string query = $"SELECT CompanyID, CompanyName FROM Companies WHERE UserID = {userId}";

            var companies = new List<object>(); // To store the results

            try
            {
                using (var connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CRMDB;Integrated Security=True;"))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    // Execute the query and read the results
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(new
                            {
                                CompanyID = reader["CompanyID"],
                                CompanyName = reader["CompanyName"].ToString()
                            });
                        }
                    }
                }

                // Return the list of companies as a JSON response
                return Ok(companies);
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 response
                Console.WriteLine($"Error fetching companies for userId {userId}: {ex.Message}");
                return StatusCode(500, "Error fetching companies");
            }
        }
        [HttpPost("AddCompany")]
        public IActionResult AddCompany([FromBody] AddCompanyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CompanyName) || request.UserId <= 0)
            {
                return BadRequest("Invalid company data.");
            }

            try
            {
                string query = $"INSERT INTO Companies (CompanyName, UserID) VALUES ('{request.CompanyName}', {request.UserId})";

                Repository repository = new Repository();
                repository.Execute(query);

                // Return a 200 OK response without content
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding company: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, "Error adding company.");
            }

        }

        [HttpGet("GetUserContacts")]
        public IActionResult GetUserContacts(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            try
            {
                string query = $"SELECT Contacts.ContactID, Contacts.ContactName, Contacts.Email, Contacts.PhoneNumber, Contacts.CompanyID, Companies.CompanyName FROM Contacts INNER JOIN Companies ON Contacts.CompanyID = Companies.CompanyID WHERE Companies.UserID = {userId}";

                Repository repository = new Repository();
                DataTable resultTable = repository.GetDataTable(query);

                if (resultTable.Rows.Count > 0)
                {
                    var contacts = new List<object>();

                    foreach (DataRow row in resultTable.Rows)
                    {
                        contacts.Add(new
                        {
                            ContactID = Convert.ToInt32(row["ContactID"]),
                            ContactName = row["ContactName"].ToString(),
                            Email = row["Email"].ToString(),
                            PhoneNumber = row["PhoneNumber"].ToString(), // Updated to match the correct column name
                            CompanyID = Convert.ToInt32(row["CompanyID"]),
                            CompanyName = row["CompanyName"].ToString()
                        });
                    }

                    return Ok(contacts);
                }
                else
                {
                    return Ok(new List<object>()); // Return an empty list if no contacts found
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddContact")]
        public IActionResult AddContact([FromBody] Contact contact)
        {
            Repository repository = new Repository();

            if (contact == null)
            {
                return BadRequest("Contact data is null.");
            }

            string sql = $"INSERT INTO Contacts (ContactName, Email, PhoneNumber, CompanyID, UserID) VALUES ('{contact.ContactName}', '{contact.Email}', '{contact.PhoneNumber}', {contact.CompanyID}, {contact.UserID})";

            try
            {
                repository.Execute(sql);
                return Ok("Contact added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding contact: {ex.Message}");
                return StatusCode(500, $"An error occurred while adding the contact: {ex.Message}");
            }
        }
    }

}

