FROM golang:1.15.2

WORKDIR /go/src/app
COPY . .
RUN go install ./...

CMD ["/go/bin/app"]