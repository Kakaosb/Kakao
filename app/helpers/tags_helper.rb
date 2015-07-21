module TagsHelper

 def find_tags 
		
	@article.text = @article.text+" "
	reg = @article.text.scan(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/)
	
	while reg != [] do
		reg = reg [0] 
		reg = reg.to_s[3...-2]

#добавление тега в БД

		@article.text = @article.text.sub(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/ , "<a href=\"tags/5\"> ##{reg}</a>")
		reg = @article.text.scan(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/)
	end 
 end

end