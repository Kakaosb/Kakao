module ArticlesHelper


def search_tag (article)
str = article.text
str_re = /#[\w\d\-_]/i
rez = str_re.match(str)
  end
end
