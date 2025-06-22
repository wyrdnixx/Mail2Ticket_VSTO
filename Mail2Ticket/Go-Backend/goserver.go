// main.go
package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"

	_ "github.com/go-sql-driver/mysql"
	"github.com/joho/godotenv"
)

var db *sql.DB

type Ticket struct {
	Type   string `json:"type"`	
	Nummer string `json:"tn"`
	Title  string `json:"title"`
	
}

func suggestionsHandler(w http.ResponseWriter, r *http.Request) {

	// Add CORS headers
	w.Header().Set("Access-Control-Allow-Origin", "https://mail.ulewu.de")
	w.Header().Set("Access-Control-Allow-Methods", "GET, OPTIONS")
	w.Header().Set("Access-Control-Allow-Headers", "Content-Type")

	// Handle preflight request
	if r.Method == http.MethodOptions {
		w.WriteHeader(http.StatusNoContent)
		return
	}

	q := r.URL.Query().Get("q")
	mail := r.URL.Query().Get("mail")
	if len(q) < 1 {
		http.Error(w, "Query param 'q' min 1 Zeichen", http.StatusBadRequest)
		return
	}
	fmt.Println("Received query:", q)
	fmt.Println("Received mail:", mail)
	searchTerm := "%" + q + "%"

	/*
	MariaDB [otobo]> desc ticket;
+--------------------------+--------------+------+-----+---------+----------------+
| Field                    | Type         | Null | Key | Default | Extra          |
+--------------------------+--------------+------+-----+---------+----------------+
| id                       | bigint(20)   | NO   | PRI | NULL    | auto_increment |
| tn                       | varchar(50)  | NO   | UNI | NULL    |                |
| title                    | varchar(191) | YES  | MUL | NULL    |                |
| queue_id                 | int(11)      | NO   | MUL | NULL    |                |
| ticket_lock_id           | smallint(6)  | NO   | MUL | NULL    |                |
| type_id                  | smallint(6)  | YES  | MUL | NULL    |                |
| service_id               | int(11)      | YES  | MUL | NULL    |                |
| sla_id                   | int(11)      | YES  | MUL | NULL    |                |
| user_id                  | int(11)      | NO   | MUL | NULL    |                |
| responsible_user_id      | int(11)      | NO   | MUL | NULL    |                |
| ticket_priority_id       | smallint(6)  | NO   | MUL | NULL    |                |
| ticket_state_id          | smallint(6)  | NO   | MUL | NULL    |                |
| customer_id              | varchar(150) | YES  | MUL | NULL    |                |
| customer_user_id         | varchar(191) | YES  | MUL | NULL    |                |
| timeout                  | int(11)      | NO   | MUL | NULL    |                |
| until_time               | int(11)      | NO   | MUL | NULL    |                |
| escalation_time          | int(11)      | NO   | MUL | NULL    |                |
| escalation_update_time   | int(11)      | NO   | MUL | NULL    |                |
| escalation_response_time | int(11)      | NO   | MUL | NULL    |                |
| escalation_solution_time | int(11)      | NO   | MUL | NULL    |                |
| archive_flag             | smallint(6)  | NO   | MUL | 0       |                |
| create_time              | datetime     | NO   | MUL | NULL    |                |
| create_by                | int(11)      | NO   | MUL | NULL    |                |
| change_time              | datetime     | NO   |     | NULL    |                |
| change_by                | int(11)      | NO   | MUL | NULL    |                |
+--------------------------+--------------+------+-----+---------+----------------+
25 rows in set (0.006 sec)

	 MariaDB [otobo]> select * from ticket_state;
+----+---------------------+----------------------------------------+---------+----------+---------------------+-----------+---------------------+-----------+
| id | name                | comments                               | type_id | valid_id | create_time         | create_by | change_time         | change_by |
+----+---------------------+----------------------------------------+---------+----------+---------------------+-----------+---------------------+-----------+
|  1 | new                 | New ticket created by customer.        |       1 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  2 | closed successful   | Ticket is closed successful.           |       3 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  3 | closed unsuccessful | Ticket is closed unsuccessful.         |       3 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  4 | open                | Open tickets.                          |       2 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  5 | removed             | Customer removed ticket.               |       6 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  6 | pending reminder    | Ticket is pending for agent reminder.  |       4 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  7 | pending auto close+ | Ticket is pending for automatic close. |       5 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  8 | pending auto close- | Ticket is pending for automatic close. |       5 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
|  9 | merged              | State for merged tickets.              |       7 |        1 | 2025-06-09 12:34:41 |         1 | 2025-06-09 12:34:41 |         1 |
+----+---------------------+----------------------------------------+---------+----------+---------------------+-----------+---------------------+-----------+
9 rows in set (0.001 sec)
	*/
	
	rows, err := db.Query(`
    SELECT 
        ticket_type.name AS type, 
        tn, 
        ticket.title
    FROM ticket 
    LEFT JOIN ticket_type ON ticket.type_id = ticket_type.id 
    LEFT JOIN customer_user ON ticket.customer_user_id = customer_user.login
    LEFT JOIN ticket_state ON ticket.ticket_state_id = ticket_state.id
    WHERE 
	ticket.ticket_state_id IN (1, 4, 6) AND (
			tn LIKE ? 
			OR ticket.title LIKE ?		
			OR customer_user.email like ?
    )    
    LIMIT 20;`,  searchTerm,searchTerm, mail)
	
	if err != nil {
		http.Error(w, "DB-Fehler", http.StatusInternalServerError)
		log.Println("DB query error:", err)
		return
	}
	defer rows.Close()

	var list []Ticket
	for rows.Next() {
		var t Ticket
		if err := rows.Scan( &t.Type, &t.Nummer, &t.Title); err != nil {
			continue
		}
		list = append(list, t)
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(list)
}

func main() {
	fmt.Printf("Starting server...\n")
	var err error

		// Load .env file
	err = godotenv.Load(".env")
	if err != nil {
		log.Println("No .env file found or error loading it, proceeding with env vars")
	} else {
		log.Println(".env file found - using for DNS.")
	}

	// Get DSN from environment
	dsn := os.Getenv("MYSQL_DSN")
	if dsn == "" {
		fmt.Printf("No settings found. Using default DSN\n")
		dsn = "otobo:P351fpLqcS0gosk4@tcp(ncl1.chaos.local:3306)/otobo?parseTime=true"
	}


	fmt.Printf("Connecting to database... \n")
	db, err = sql.Open("mysql", dsn)
	if err != nil {
		log.Fatal(err)
	}
	defer db.Close()

	http.HandleFunc("/api/tickets/suggestions", suggestionsHandler)

	log.Println("Server läuft auf :8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
