package http

import (
	"bytes"
	"io"
	"log"
	"net/http"
)

type ResponseRecorder struct {
	http.ResponseWriter
	statusCode int
	body       *bytes.Buffer
}

func (r *ResponseRecorder) WriteHeader(statusCode int) {
	r.statusCode = statusCode
	r.ResponseWriter.WriteHeader(statusCode)
}

func (r *ResponseRecorder) Write(b []byte) (int, error) {
	r.body.Write(b)
	return r.ResponseWriter.Write(b)
}

func (r *ResponseRecorder) GetBody() string {
	return r.body.String()
}

func LogMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Request: %s %s", r.Method, r.URL.Path)
		log.Printf("Request Headers: %v", r.Header)

		var requestBody bytes.Buffer
		teeReader := io.TeeReader(r.Body, &requestBody)
		r.Body = io.NopCloser(teeReader)

		recorder := &ResponseRecorder{
			ResponseWriter: w,
			body:           &bytes.Buffer{},
		}

		next(recorder, r)

		log.Printf("Request Body: %s", requestBody.String())

		log.Printf("Response Status: %d", recorder.statusCode)
		log.Printf("Response Headers: %v", recorder.Header())
		log.Printf("Response Body: %s", recorder.GetBody())
	}
}
