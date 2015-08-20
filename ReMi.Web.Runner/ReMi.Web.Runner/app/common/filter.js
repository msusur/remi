(function () {
    'use strict';

    angular.module('common').filter('formatenum', function () {
        return function (input, array) {
            if (!array) return '';

            input = input || '';

            var found = '';
            array.forEach(function (item) {
                if ((item.Value && item.Value === input)
                    || (item.Name && item.Name === input)) {
                    found = item.Text ? item.Text : item.Description;
                    return;
                }
            });

            return found;
        };
    });

    angular.module('common').filter('remilinky', ['$sanitize', function ($sanitize) {
        var uriAttrs = makeMap("background,cite,href,longdesc,src,usemap");
        var validAttrs = angular.extend({}, uriAttrs, makeMap(
            'abbr,align,alt,axis,bgcolor,border,cellpadding,cellspacing,class,clear,' +
            'color,cols,colspan,compact,coords,dir,face,headers,height,hreflang,hspace,' +
            'ismap,lang,language,nohref,nowrap,rel,rev,rows,rowspan,rules,' +
            'scope,scrolling,shape,size,span,start,summary,target,title,type,' +
            'valign,value,vspace,width'));

        function makeMap(str) {
            var obj = {}, items = str.split(','), i;
            for (i = 0; i < items.length; i++) obj[items[i]] = true;
            return obj;
        }
        var LINKY_URL_REGEXP =
              /((ftp|https?):\/\/|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s.;,(){}<>]/,
            MAILTO_REGEXP = /^mailto:/, NON_ALPHANUMERIC_REGEXP = /([^\#-~| |!])/g;;
        var specialElements = makeMap("script,style");

        return function (text, target) {
            if (!text) return text;
            var match;
            var raw = text;
            var html = [];
            var url;
            var i;
            while ((match = raw.match(LINKY_URL_REGEXP))) {
                // We can not end in these as they are sometimes found at the end of the sentence
                url = match[0];
                // if we did not match ftp/http/mailto then assume mailto
                if (match[2] == match[3]) url = 'mailto:' + url;
                i = match.index;
                addText(raw.substr(0, i));
                addLink(url, match[0].replace(MAILTO_REGEXP, ''));
                raw = raw.substring(i + match[0].length);
            }
            addText(raw);
            return $sanitize(html.join(''));

            function addText(text) {
                if (!text) {
                    return;
                }
                html.push(sanitizeText(text));
            }

            function sanitizeText(chars) {
                var buf = [];
                var writer = htmlSanitizeWriter(buf, angular.noop);
                writer.chars(chars);
                return buf.join('');
            }

            function addLink(url, text) {
                html.push('<br/><a ');
                if (angular.isDefined(target)) {
                    html.push('target="');
                    html.push(target);
                    html.push('" ');
                }
                html.push('href="');
                html.push(url);
                html.push('">');
                addText(text);
                html.push('</a>');
            }

            function htmlSanitizeWriter(buf, uriValidator) {
                var ignore = false;
                var out = angular.bind(buf, buf.push);
                return {
                    start: function (tag, attrs, unary) {
                        tag = angular.lowercase(tag);
                        if (!ignore && specialElements[tag]) {
                            ignore = tag;
                        }
                        if (!ignore && validElements[tag] === true) {
                            out('<');
                            out(tag);
                            angular.forEach(attrs, function (value, key) {
                                var lkey = angular.lowercase(key);
                                var isImage = (tag === 'img' && lkey === 'src') || (lkey === 'background');
                                if (validAttrs[lkey] === true &&
                                  (uriAttrs[lkey] !== true || uriValidator(value, isImage))) {
                                    out(' ');
                                    out(key);
                                    out('="');
                                    out(encodeEntities(value));
                                    out('"');
                                }
                            });
                            out(unary ? '/>' : '>');
                        }
                    },
                    end: function (tag) {
                        tag = angular.lowercase(tag);
                        if (!ignore && validElements[tag] === true) {
                            out('</');
                            out(tag);
                            out('>');
                        }
                        if (tag == ignore) {
                            ignore = false;
                        }
                    },
                    chars: function (chars) {
                        if (!ignore) {
                            out(encodeEntities(chars));
                        }
                    }
                };
            }

            function encodeEntities(value) {
                return value.
                  replace(/&/g, '&amp;').
                  replace(NON_ALPHANUMERIC_REGEXP, function (value) {
                      return '&#' + value.charCodeAt(0) + ';';
                  }).
                  replace(/</g, '&lt;').
                  replace(/>/g, '&gt;');
            }
        };
    }]);

})();
