document.addEventListener("DOMContentLoaded", function () {
    toggleSearchInput(); // alkuasetukset heti sivun latauksen jälkeen
});


function toggleSearchInput() {
    const type = document.getElementById('searchType').value;
    const input = document.getElementById('searchInput');
    const genre = document.getElementById('genreSelect');

    if (type === 'author' || type === 'year') {
        input.style.display = 'block';
    } else {
        input.style.display = 'none';
        input.value = '';
    }

    genre.style.display = (type === 'genre') ? 'block' : 'none';
}

document.addEventListener("DOMContentLoaded", function () {
    toggleSearchInput();
});
// Search for books based on the selected type and input
// Handles the search logic based on the selected type and input value
function searchBook() {
    const button = document.getElementById('searchButton');
    let cooldown = 3; // ⏱️ 3 sekuntia
    button.disabled = true;

    // Näytetään jäljellä oleva aika painikkeessa
    button.textContent = `Wait (${cooldown})`;

    // Päivitetään teksti sekunnin välein
    const interval = setInterval(() => {
        cooldown--;
        if (cooldown > 0) {
            button.textContent = `Wait (${cooldown})`;
        } else {
            clearInterval(interval);
            button.disabled = false;
            button.textContent = 'Search';
        }
    }, 1000);

    // 🔍 Varsinainen haku alkaa heti
    const searchType = document.getElementById('searchType').value;
    const searchInput = document.getElementById('searchInput').value.trim();
    const genre = document.getElementById('genreSelect').value;

    if (searchType === 'my-authors') {
        fetchMyAuthorsBooks();
        return;
    }

    let query = '';
    if (searchType === 'author' && searchInput) {
        query = `inauthor:${encodeURIComponent(searchInput)}`;
    } else if (searchType === 'year' && searchInput) {
        query = `subject:book+inpublisher:${encodeURIComponent(searchInput)}`;
    } else if (searchType === 'genre') {
        query = `subject:${encodeURIComponent(genre)}`;
    } else {
        query = 'book';
    }

    const apiUrl = `https://www.googleapis.com/books/v1/volumes?q=${query}&maxResults=40`;
    fetchBooks(apiUrl);
}


// Fetch books from Google Books API
// Fetches books based on the provided API URL and displays them
function fetchBooks(apiUrl) {
    fetch(apiUrl)
        .then(res => res.json())
        .then(data => {
            const books = data.items;
            if (!books || books.length === 0) {
                document.getElementById('results').innerHTML = '<p>No books found.</p>';
                return;
            }
            displayResults(books);
        })
        .catch(error => {
            console.error("Error fetching books:", error);
            document.getElementById('results').innerHTML = '<p>Something went wrong while fetching books.</p>';
        });
}
// Fetch books for authors in the user's library
// Assumes you have a backend endpoint that returns the authors in the user's library
function fetchMyAuthorsBooks() {
    fetch('/Index?handler=MyAuthors')
        .then(res => res.json())
        .then(authors => {
            if (!authors || authors.length === 0) {
                document.getElementById('results').innerHTML = '<p>No authors found in your library.</p>';
                return;
            }
            // Fetch books for each author
            // This function fetches books for each author and displays them
            const fetches = authors.map(author => {
                const query = `inauthor:${encodeURIComponent(author)}`;
                const apiUrl = `https://www.googleapis.com/books/v1/volumes?q=${query}&maxResults=40`;
                return fetch(apiUrl).then(res => res.json());
            });
            // Wait for all fetches to complete and then display results
            // Waits for all fetches to complete and then displays the results
            Promise.all(fetches)
                .then(results => {
                    const allBooks = results.flatMap(r => r.items || []);
                    if (allBooks.length === 0) {
                        document.getElementById('results').innerHTML = '<p>No books found for your authors.</p>';
                        return;
                    }
                    displayResults(allBooks);
                })
                .catch(error => {
                    console.error("Error fetching books for authors:", error);
                    document.getElementById('results').innerHTML = '<p>Something went wrong while loading books.</p>';
                });
        });
}
// Display the results of the book search
// Displays the book information in a card format
function displayResults(books) {
    const resultsDiv = document.getElementById('results');
    if (!books || books.length === 0) {
        resultsDiv.innerHTML = '<p>No books found.</p>';
        return;
    }

    // 🎲 Random book
    const book = books[Math.floor(Math.random() * books.length)];
    const volumeInfo = book.volumeInfo;

    const title = volumeInfo.title?.replace(/'/g, "\\'") || 'Unknown';
    const authors = (volumeInfo.authors || ['Unknown']).join(', ').replace(/'/g, "\\'");
    const year = volumeInfo.publishedDate?.substring(0, 4) || 'Unknown';
    const thumbnail = volumeInfo.imageLinks?.thumbnail || '/images/no-cover.png';
    const descriptionRaw = volumeInfo.description || 'No description available.';
    const description = descriptionRaw.length > 300
        ? descriptionRaw.substring(0, 300) + '...'
        : descriptionRaw;

    // ✅ Show information
    resultsDiv.innerHTML = `
    <div class="book-card">
        <div class="book-image-container">
            <img src="${thumbnail}" alt="${title}">
            <button class="add-library-btn"
                    ${isAuthenticated ? `onclick="addToLibrary('${title}', '${authors}', '${year}', '${thumbnail}', \`${description}\`)"` : 'onclick="redirectToLogin()"'}
                    title="${isAuthenticated ? 'Add this book to your library' : 'Login to save this book'}">
                📚 ${isAuthenticated ? 'Add to Library' : 'Login to Save'}
            </button>
        </div>
                                <div class="book-details">
                                    <h3>${title}</h3>
                                    <p>Author: ${authors}</p>
                                    <p>Year: ${year}</p>
                                    <p><strong>Description:</strong> ${description}</p>
                                </div>
                            </div>
                        `;
}
// Add book to library
// This function sends a POST request to add the book to the user's library
async function addToLibrary(title, author, year, thumbnail, description) {
    try {
        const response = await fetch('/Library?handler=AddBook', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value,
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                Title: title,
                Author: author,
                PublicationYear: parseInt(year) || 0,
                ThumbnailUrl: thumbnail,
                Description: description
            })
        });

        const result = await response.json();
        if (result.success) {
            alert('Book added to your library!');
            window.location.href = '/Library';
        } else {
            if (result.message.includes("5 books")) {
                alert("❌ You already have 5 books saved. Please delete one before adding another.");
            } else {
                alert(result.message || 'Failed to add book');
            }
        }
    } catch (error) {
        console.error('Error adding book:', error);
        alert('Error adding book to library');
    }
}


function redirectToLogin() {
    window.location.href = '/Login';
}
