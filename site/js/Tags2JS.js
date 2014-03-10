var Tags2JS = ( function(Tags2JS) {

		Tags2JS.init = function(host) {
			var connection = new WebSocket(host);
			// When the connection is open, send some data to the server
			connection.onopen = function() {
				console.log("connection opened: " + host);
			};

			// Log errors
			connection.onerror = function(error) {
				console.log("Error: ");
				console.log(error);
			};

			// Log messages from the server
			connection.onmessage = function(e) {
				var tagObject = JSON.parse(e.data);
				if (tagObject.eventType === "tag_down") {
					Tags2JS.createTagVisualization(tagObject);
				}else if(tagObject.eventType === "tag_move"){
					var div = document.getElementById("tag_" + tagObject.id);
					div.placeAt(tagObject.x, tagObject.y);
				}else if(tagObject.eventType === "tag_up"){
					var div = document.getElementById("tag_" + tagObject.id);
					div.parentNode.removeChild(div);
				}
			};

			connection.onclose = function() {
				console.log("connection closed: " + host);
			};
		};

		Tags2JS.createTagVisualization = function(tagObject) {
			var canvas = document.getElementById("content"), div = document.createElement("div");
			div.id = "tag_" + tagObject.id;
			div.className = "tagDiv";
			div.style.background = RandomColors.go();
			div.placeAt = function(x, y){
				div.style.top = y + "px";
				div.style.left = x + "px";
			}	
				
			div.placeAt(tagObject.x, tagObject.y);
			
			document.body.appendChild(div);

			return div;
		};
		
		return Tags2JS;
	}(Tags2JS || {}));
	
var RandomColors = {
	go: function(){
		var color = "#",
			r = Math.floor(Math.random()*256).toString(16),
			g = Math.floor(Math.random()*256).toString(16),
			b = Math.floor(Math.random()*256).toString(16);
		r = r.length === 1 ? "0"+r : r;
		g = g.length === 1 ? "0"+g : g;
		b = b.length === 1 ? "0"+b : b;
		color = color + r + g + b;
		return color;
	}
};
